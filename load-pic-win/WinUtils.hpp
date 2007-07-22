#ifndef _WIN_UTILS_HPP_
#define _WIN_UTILS_HPP_

#include <windows.h>
#include <ole2.h>
#include <ocidl.h>   // LPPICTURE

int     RectDx(RECT *rc);
int     RectDy(RECT *rc);
void    RectSetDx(RECT *rc, int dx);
void    RectSetDy(RECT *rc, int dy);

BOOL    PointInsideRect(int x, int y, RECT *rc);
BOOL    PointInsideRect(int x, int y, int rcX, int rcY, int rcDx, int rcDy);

int     SizeDx(SIZE *size);
int     SizeDy(SIZE *size);
void    SizeSetDxDy(SIZE *size, int dx, int dy);

void    FillRectangle(HDC dc, RECT *rc, COLORREF col);
void    FillRectangle(HDC dc, int x, int y, int dx, int dy, COLORREF col);

IPicture *  LoadPictureViaOle(TCHAR* filePath);
void        GetPixelSizeFromPicture(IPicture *pic, HDC hdc, SIZE *size);
#endif

