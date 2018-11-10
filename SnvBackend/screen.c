#include "screen.h"

#include <Windows.h>

#define SETRES(d) (d ? d : 1)
#define WH_ALIGN(v) ((int)(v / 8) * 8)

BOOL CALLBACK enum_dspl_callback(HMONITOR m, HDC hm, LPRECT rm, LPARAM data) {
	(*((int*)data))++;
	return TRUE;
}

int scr_count_displays() {
	int count = 0;
	if (EnumDisplayMonitors(NULL, NULL, enum_dspl_callback, (LPARAM)&count)) {
		return count;
	}

	return -1;
}

int scr_get_resolution(int display)
{
	DISPLAY_DEVICE dev;
	dev.cb = sizeof(DISPLAY_DEVICE);
	if (!EnumDisplayDevices(NULL, display, &dev, 0)) {
		return 0;
	}

	HDC hDisplay = CreateDC("DISPLAY", dev.DeviceName, NULL, NULL);
	if (hDisplay == NULL) {
		return 0;
	}

	int res = (GetDeviceCaps(hDisplay, HORZRES) << 16) | (uint16_t)GetDeviceCaps(hDisplay, VERTRES);
	DeleteDC(hDisplay);
	return res;
}

int scr_capture(s2_frame *frm, int display, unsigned int den) {
	if (!den) {
		return 1;
	}

	DISPLAY_DEVICE dev;
	dev.cb = sizeof(DISPLAY_DEVICE);
	if (!EnumDisplayDevices(NULL, display, &dev, 0)) {
		return 2;
	}

	HDC hDisplay = CreateDC("DISPLAY", dev.DeviceName, NULL, NULL);
	if (hDisplay == NULL) {
		return 3;
	}
	
	const int sw = GetDeviceCaps(hDisplay, HORZRES);
	const int sh = GetDeviceCaps(hDisplay, VERTRES);
	const int w_raw = SETRES(sw / den);
	const int w_align = WH_ALIGN(w_raw);

	s2_alloc_frame(frm, w_align, SETRES(sh / den));
	if (frm->error) {
		return 4;
	}

	HDC hMem = CreateCompatibleDC(hDisplay);
	HBITMAP hBitmap = CreateCompatibleBitmap(hDisplay, frm->w, frm->h);
	HGDIOBJ hOld = SelectObject(hMem, hBitmap);

	if (den == 1 && w_raw == w_align) {
		BitBlt(hMem, 0, 0, frm->w, frm->h, hDisplay, 0, 0, SRCCOPY);
	}
	else {
		SetStretchBltMode(hMem, HALFTONE);
		StretchBlt(hMem, 0, 0, frm->w, frm->h, hDisplay, 0, 0, sw, sh, SRCCOPY);
	}

	SelectObject(hMem, hOld);

	BITMAPINFOHEADER bmi;
	bmi.biSize = sizeof(BITMAPINFOHEADER);
	bmi.biPlanes = 1;
	bmi.biBitCount = 24;
	bmi.biWidth = frm->w;
	bmi.biHeight = -frm->h;
	bmi.biCompression = BI_RGB;
	bmi.biSizeImage = 0;

	GetDIBits(hMem, hBitmap, 0, frm->h, frm->pixels, (BITMAPINFO*)&bmi, DIB_RGB_COLORS);

	DeleteDC(hDisplay);
	DeleteDC(hMem);
	DeleteObject(hBitmap);

	return 0;
}