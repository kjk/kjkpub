#include <assert.h>
#include "WinUtils.hpp"
#include <olectl.h>  // OleLoadPicture()

int RectDx(RECT *rc)
{
    assert(rc->right >= rc->left);
    return rc->right - rc->left;
}

int RectDy(RECT *rc)
{
    assert(rc->bottom >= rc->top);
    return rc->bottom - rc->top;
}

void RectSetDx(RECT *rc, int dx)
{
    assert(rc->left >= 0);
    rc->right = rc->left + dx;
}

void RectSetDy(RECT *rc, int dy)
{
    assert(rc->top >= 0);
    rc->bottom = rc->top + dy;
}

BOOL PointInsideRect(int x, int y, RECT *rc)
{
    if ( (x < rc->left) || (x > rc->right))
        return FALSE;
    if ( (y < rc->top) || (y > rc->bottom) )
        return FALSE;

    return TRUE;
}

BOOL PointInsideRect(int x, int y, int rcX, int rcY, int rcDx, int rcDy)
{
    if ( (x < rcX) || (x > rcX + rcDx))
        return FALSE;
    if ( (y < rcY) || (y > rcY + rcDy) )
        return FALSE;

    return TRUE;
}

int SizeDx(SIZE *size) { return size->cx; }
int SizeDy(SIZE *size) { return size->cy; }
void SizeSetDxDy(SIZE *size, int dx, int dy)
{
    size->cx = dx;
    size->cy = dy;
}

void FillRectangle(HDC dc, RECT *rc, COLORREF col)
{
    HPEN    pen = NULL;
    HBRUSH  br = NULL;

    pen = CreatePen(PS_SOLID, 1, col);
    if (NULL == pen)
        goto Exit;
    br = CreateSolidBrush(col);
    if (NULL == br)
        goto Exit;
        
    HBRUSH prevBrush = (HBRUSH)SelectObject(dc, br);
    HPEN prevPen = (HPEN)SelectObject(dc, pen);
    Rectangle(dc, rc->left, rc->top, rc->right, rc->bottom);
    SelectObject(dc, prevBrush);
    SelectObject(dc, prevPen);

Exit:
    if (NULL != pen)
        DeleteObject(pen);
    if (NULL != br)
        DeleteObject(br);
}

void    FillRectangle(HDC dc, int x, int y, int dx, int dy, COLORREF col)
{
    RECT rc = { x, y, x+dx, y+dy};
    FillRectangle(dc, &rc, col);
}

#define HIMETRIC_INCH	2540
#define MAP_LOGHIM_TO_PIX(x,ppli)   ( ((ppli)*(x) + HIMETRIC_INCH/2) / HIMETRIC_INCH )

void GetPixelSizeFromPicture(IPicture *pic, HDC hdc, SIZE *size)
{
    long picDx, picDy;
    int  pixelDx, pixelDy;
    pic->get_Width(&picDx);
    pic->get_Height(&picDy);
    // convert himetric to pixels
    pixelDx	= (int)MulDiv(picDx, GetDeviceCaps(hdc, LOGPIXELSX), HIMETRIC_INCH);
    pixelDy	= (int)MulDiv(picDy, GetDeviceCaps(hdc, LOGPIXELSY), HIMETRIC_INCH);
    
    SizeSetDxDy(size, pixelDx, pixelDy);
}

static HGLOBAL GetFileDataInGlobalMem(TCHAR *fileName)
{
    HGLOBAL    hGlobal = NULL;
    HANDLE     hFile = INVALID_HANDLE_VALUE;
    DWORD      dwFileSize;
    DWORD      dwBytesRead;
    void *     fileData;

    hFile = CreateFile(fileName, GENERIC_READ, 0, NULL, OPEN_EXISTING, 0, NULL);
    if (INVALID_HANDLE_VALUE == hFile)
        return NULL;

    dwFileSize = GetFileSize(hFile,NULL);
    if (dwFileSize == -1)
        goto Exit;

    hGlobal = GlobalAlloc(GMEM_MOVEABLE, dwFileSize);
    if (NULL == hGlobal)
        goto Exit;

    fileData = GlobalLock(hGlobal);
    ReadFile(hFile, fileData, dwFileSize, &dwBytesRead, NULL);
    assert(dwBytesRead == dwFileSize);
Exit:
    if (NULL != hGlobal)
        GlobalUnlock(hGlobal);
    if (INVALID_HANDLE_VALUE != hFile)
        CloseHandle(hFile);
    return hGlobal;
}

IPicture* LoadPictureViaOle(TCHAR* filePath)
{
    IPicture *  pic = NULL;
    HRESULT     hr;
    LPSTREAM    pStream = NULL;
    HGLOBAL     hGlobal = NULL;
    OLE_XSIZE_HIMETRIC dx, dy;

    hGlobal = GetFileDataInGlobalMem(filePath);
    if (NULL == hGlobal)
        return NULL;

    hr = CreateStreamOnHGlobal(hGlobal, TRUE, &pStream);
    if (FAILED(hr))
        goto Exit;

    hr = OleLoadPicture(pStream, 0, FALSE, IID_IPicture, (void**)&pic);
    if (FAILED(hr))
        goto Exit;

    assert(NULL != pic);
    pic->get_Width(&dx);
    pic->get_Height(&dy);

Exit:
    if (NULL != pStream)
        pStream->Release();
    if (NULL != hGlobal)
        GlobalFree(hGlobal);    
    return pic;
}

