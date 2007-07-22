#define STRICT

#include "WinUtils.hpp"
#include "Window.hpp"

#include <assert.h>
#include <windowsx.h>
#include <commctrl.h>
#include <shlwapi.h>
#include <shlobj.h>
#include <shellapi.h>

HINSTANCE   g_hinst = NULL;
IPicture*   g_pic = NULL;
BOOL        g_fAnimate = FALSE;

#define COL_WHITE RGB(255,255,255)
#define COL_RED RGB(255, 0, 0)
#define COL_BLUE RGB(0, 0, 255)

#define ANIM_TIMER_ID 10
#define PIXELS_TO_TRAVEL 10

enum AnimDirEnum {
    AnimDirLeft = 1,
    AnimDirRight = 2
};

#define KEY_MOVE_SPEED 6

class RootWindow : public Window
{
public:
    typedef Window super;
    virtual LPCTSTR ClassName() { return TEXT("Show picture"); }
    static RootWindow *Create(int dx, int dy, BOOL client);
protected:
    LRESULT HandleMessage(UINT uMsg, WPARAM wParam, LPARAM lParam);
    LRESULT OnCreate();
    virtual void PaintContent(PAINTSTRUCT *pps);
    virtual void OnSize(int dx, int dy);
    virtual LRESULT OnEraseBackground(void) { return 1; }
    LRESULT OnMouseMove(WPARAM key, int x, int y);

    void DrawPict(HDC hdc);
    void DrawPictCentered(HDC hdc);
    void OnTimer(void);
    void UpdatePict(RECT *rcClient);
    void OnKeyDown(WPARAM keyCode, LPARAM flags);

private:
    HWND m_hwndChild;
public:
    int     m_dx;
    int     m_dy;
    BOOL    m_client;
    BOOL    m_fPictHasBorder;
    int     m_pictPixelDx;
    int     m_pictPixelDy;
    int     m_pictPosX;
    int     m_pictPosY;

    int             m_count;
    AnimDirEnum     m_direction;
};

void RootWindow::UpdatePict(RECT *rcClient)
{
    if (m_direction == AnimDirLeft)
    {
        --m_count;
        m_pictPosX -= 1;
        if (m_count == -PIXELS_TO_TRAVEL)
            m_direction = AnimDirRight;
    }
    else
    {
        ++m_count;
        m_pictPosX += 1;
        if (m_count == PIXELS_TO_TRAVEL)
            m_direction = AnimDirLeft;
    }
}

void RootWindow::OnTimer(void)
{
    RECT    rcClient;
    HDC     hdc = GetDC(m_hwnd);

    GetClientRect(m_hwnd, &rcClient);

    UpdatePict(&rcClient);
    DrawPict(hdc);
    ReleaseDC(m_hwnd, hdc);
}

void RootWindow::OnSize(int dx, int dy)
{
    if (NULL != m_hwndChild) 
    {
        SetWindowPos(m_hwndChild, NULL, 0, 0,
                 dx, dy,
                 SWP_NOZORDER | SWP_NOACTIVATE);
    }
    InvalidateRect(m_hwnd, NULL, FALSE);
    UpdateWindow(m_hwnd);
}

#define SHIFTED 0x8000 

void RootWindow::OnKeyDown(WPARAM keyCode, LPARAM flags)
{
    BOOL    fPosChanged = FALSE;
    HDC     hdc;
    int     nVirtKey;
    int     multiplier = 1;

    nVirtKey = GetKeyState(VK_SHIFT); 
    if (nVirtKey & SHIFTED)
        multiplier = 2;


    if (VK_LEFT == keyCode)
    {
        m_pictPosX -= KEY_MOVE_SPEED * multiplier;
        fPosChanged = TRUE;
    }
    else if (VK_RIGHT == keyCode)
    {
        m_pictPosX += KEY_MOVE_SPEED * multiplier;
        fPosChanged = TRUE;
    }
    else if (VK_UP == keyCode)
    {
        m_pictPosY -= KEY_MOVE_SPEED * multiplier;
        fPosChanged = TRUE;
    }
    else if (VK_DOWN == keyCode)
    {
        m_pictPosY += KEY_MOVE_SPEED * multiplier;
        fPosChanged = TRUE;
    }

    if (!fPosChanged)
        return;

    hdc = GetDC(m_hwnd);
    DrawPict(hdc);
    ReleaseDC(m_hwnd, hdc);
}

LRESULT RootWindow::OnCreate()
{
    SIZE    pictSize;
    if (NULL != g_pic)
    {
        HDC dc = GetDC(m_hwnd);
        GetPixelSizeFromPicture(g_pic, dc, &pictSize);
        ReleaseDC(m_hwnd, dc);
        m_pictPixelDx = SizeDx(&pictSize);
        m_pictPixelDy = SizeDy(&pictSize);
        if ((0 != m_pictPixelDx) && (0 != m_pictPixelDy))
        {
            SetClientRectSize(m_pictPixelDx, m_pictPixelDy);
            if (g_fAnimate)
                SetTimer(m_hwnd, ANIM_TIMER_ID, 50, NULL);
            return 0;
        }
    }

    if (m_client)
        SetClientRectSize(m_dx, m_dy);
    else
        SetSize(m_dx, m_dy);

    if (g_fAnimate)
        SetTimer(m_hwnd, ANIM_TIMER_ID, 50, NULL);
    return 0;
}

#define BORDER_PIXEL_SIZE 3

void RootWindow::DrawPictCentered(HDC hdc)
{
    RECT    clientRc;
    int     winDx, winDy;

    if (NULL == g_pic)
        return;

    GetClientRect(m_hwnd, &clientRc);
    winDx = RectDx(&clientRc);
    winDy = RectDy(&clientRc);

    m_pictPosX = 0;
    if (winDx > m_pictPixelDx)
        m_pictPosX = (winDx - m_pictPixelDx) / 2;

    m_pictPosY = 0;
    if (winDy > m_pictPixelDy)
        m_pictPosY = (winDy - m_pictPixelDy) / 2;

    m_count = 0;
    m_direction = AnimDirRight;

    DrawPict(hdc);
}

void RootWindow::DrawPict(HDC hdc)
{
    RECT    clientRc;
    long    picDx, picDy;
    int     winDx, winDy;
    HDC     dcTmp = NULL;
    HBITMAP bmpTmp = NULL;
    HBITMAP bmpOld = NULL;

    if (NULL == g_pic)
        return;

    GetClientRect(m_hwnd, &clientRc);
    winDx = RectDx(&clientRc);
    winDy = RectDy(&clientRc);

    g_pic->get_Width(&picDx);
    g_pic->get_Height(&picDy);

    dcTmp = CreateCompatibleDC(hdc);
    if (NULL == dcTmp)
        return;

    bmpTmp = CreateCompatibleBitmap(hdc, winDx, winDy);
    if (NULL == bmpTmp)
        goto Exit;
    bmpOld = (HBITMAP)SelectObject(dcTmp, bmpTmp);

    HDC dcToDraw = dcTmp;

    FillRectangle(dcToDraw, 0, 0, RectDx(&clientRc), RectDy(&clientRc), COL_WHITE);

    if (m_fPictHasBorder)
    {
        RECT tmpRc = { 0 };
        tmpRc.left = m_pictPosX - BORDER_PIXEL_SIZE;
        tmpRc.top  = m_pictPosY - BORDER_PIXEL_SIZE;
        tmpRc.right = tmpRc.left + m_pictPixelDx + 2*BORDER_PIXEL_SIZE;
        tmpRc.bottom = tmpRc.top + m_pictPixelDy + 2*BORDER_PIXEL_SIZE;
        FillRectangle(dcToDraw, &tmpRc, COL_RED);
    }

    g_pic->Render(dcToDraw, 
                  m_pictPosX, m_pictPosY, 
                  m_pictPixelDx, m_pictPixelDy,
                  0, picDy, 
                  picDx, -picDy, NULL);

    BitBlt(hdc, 0, 0, winDx, winDy, dcToDraw, 0, 0, SRCCOPY);

    SelectObject(dcTmp, bmpOld);

Exit:
    if (NULL != bmpTmp)
        DeleteObject(bmpTmp);
    if (NULL != dcTmp)
        DeleteDC(dcTmp);
}

void RootWindow::PaintContent(PAINTSTRUCT *pps)
{
    DrawPictCentered(pps->hdc);
}

LRESULT RootWindow::OnMouseMove(WPARAM key, int x, int y)
{
    BOOL    fShouldHaveBorder = PointInsideRect(x, y, m_pictPosX, m_pictPosY, m_pictPixelDx, m_pictPixelDy);
    if (fShouldHaveBorder == m_fPictHasBorder)
        return 0;

    m_fPictHasBorder = fShouldHaveBorder;
    InvalidateRect(m_hwnd, NULL, FALSE);
    UpdateWindow(m_hwnd);
    return 0;
}

LRESULT RootWindow::HandleMessage(UINT uMsg, WPARAM wParam, LPARAM lParam)
{
    switch (uMsg) 
    {
        case WM_CREATE:
            return OnCreate();  

        case WM_MOUSEMOVE:
            return OnMouseMove(wParam, GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam));
        
        case WM_NCDESTROY:
            KillTimer(m_hwnd, ANIM_TIMER_ID);
            // Death of the root window ends the thread
            PostQuitMessage(0);
            break;

        case WM_SETFOCUS:
            if (NULL != m_hwndChild)
                SetFocus(m_hwndChild);
            return 0;

        case WM_TIMER:
            OnTimer();
            return 0;

        case WM_KEYDOWN:
            OnKeyDown(wParam, lParam);
            return 0;
    }
    
    return super::HandleMessage(uMsg, wParam, lParam);
}

RootWindow *RootWindow::Create(int dx, int dy, BOOL client)
{
    RootWindow *self = new RootWindow();
    if (NULL == self)
        return NULL;

    self->m_dx = dx;
    self->m_dy = dy;
    self->m_client = client;
    self->m_hwndChild = NULL;
    self->m_fPictHasBorder = FALSE;
    self->m_count = 0;
    self->m_direction = AnimDirLeft;

    if (!self->WinCreateWindow(0,
       TEXT("Show Picture"), WS_OVERLAPPEDWINDOW,
       CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT,
       NULL, NULL)) 
    {
        delete self;
        return NULL;
    }
    return self;
}

#define PIC_FILE_NAME TEXT("img.jpg")

int PASCAL
WinMain(HINSTANCE hinst, HINSTANCE, LPSTR, int nShowCmd)
{
    BOOL    fOk;

    g_hinst = hinst;
    if (FAILED(CoInitialize(NULL))) 
        return 0;

    g_pic = LoadPictureViaOle(PIC_FILE_NAME);

    InitCommonControls();
    RootWindow *prw = RootWindow::Create(20, 50, TRUE /* client */);
    if (NULL == prw)
        goto Exit;
    ShowWindow(prw->GetHWND(), nShowCmd);

    MSG msg;
    while (GetMessage(&msg, NULL, 0, 0)) {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }
Exit:
    CoUninitialize();
    return 0;
}
