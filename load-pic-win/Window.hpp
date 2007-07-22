#ifndef _WINDOW_HPP_
#define _WINDOW_HPP_

#include <windows.h>

class Window
{
public:
    HWND GetHWND() { return m_hwnd; }
    void SetSize(int dx, int dy);
    void SetClientRectSize(int dx, int dy);

protected:
    virtual LRESULT HandleMessage(UINT uMsg, WPARAM wParam, LPARAM lParam);
    virtual void OnSize(int dx, int dy) { }
    virtual LRESULT OnEraseBackground(void) { return 1; }

    virtual void PaintContent(PAINTSTRUCT *pps) { }
    virtual LPCTSTR ClassName() = 0;
    virtual BOOL WinRegisterClass(WNDCLASS *pwc)
     { return RegisterClass(pwc); }
    virtual ~Window() { }

    HWND WinCreateWindow(DWORD dwExStyle, LPCTSTR pszName,
       DWORD dwStyle, int x, int y, int cx, int cy,
       HWND hwndParent, HMENU hmenu);

private:
    void Register();
    void OnPaint();
    void OnPrintClient(HDC hdc);
    static LRESULT CALLBACK s_WndProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);
protected:
    HWND m_hwnd;
};


#endif
