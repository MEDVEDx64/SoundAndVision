#include <Windows.h>
#include <stdio.h>
#include <time.h>

#include "snv.h"
#include "resource.h"
#include "recorder.h"

// A separate file with everything related to client notifyicon

#define ICONID (GUID){0xd6f6b114, 0xbdca, 0x4167, {0xae, 0x4b, 0x2d, 0xfa, 0x88, 0xf2, 0x11, 0xca}}
#define ICONID_PATCH(id) id.Data1 = (unsigned long)id_mod

#define WM_SNV_QUIT (WM_APP + 1)
#define WM_SNV_REC_START (WM_APP + 2)
#define WM_SNV_REC_STOP (WM_APP + 3)
#define WM_SNV_REC_SETTINGS (WM_APP + 4)

static void(*ni_callback)() = NULL;
static HWND ni_wnd = NULL;
static time_t id_mod = 0;
static UINT WM_TASKBARCREATED = 0;

void setup_icon(HWND wnd) {
	NOTIFYICONDATAW ni;
	memset(&ni, 0, sizeof(ni));
	HINSTANCE inst = GetModuleHandle("snvbackend.dll");

	ni.cbSize = sizeof(ni);
	ni.hWnd = wnd;
	ni.uCallbackMessage = WM_APP;
	ni.uFlags = NIF_ICON | NIF_GUID | NIF_TIP | NIF_MESSAGE;
	ni.hIcon = LoadIcon(inst, MAKEINTRESOURCE(IDI_ICON1));
	ni.guidItem = ICONID;
	ICONID_PATCH(ni.guidItem);
	sprintf(ni.szTip, "%s", "SnvClient");

	Shell_NotifyIcon(NIM_ADD, &ni);
}

void snv_show_notification(const wchar_t *text, int beep) {
	if (text == NULL) {
		return;
	}

	NOTIFYICONDATAW ni;

	memset(&ni, 0, sizeof(ni));
	ni.cbSize = sizeof(ni);
	ni.guidItem = ICONID;
	ICONID_PATCH(ni.guidItem);
	ni.uFlags = NIF_GUID | NIF_INFO;

	wcsncpy(ni.szInfo, text, 255);
	swprintf(ni.szInfoTitle, 63, L"SnvClient - Message");
	ni.dwInfoFlags = NIIF_INFO | NIIF_NOSOUND;

	Shell_NotifyIconW(NIM_MODIFY, &ni);
	if (beep) {
		PlaySound((LPCTSTR)SND_ALIAS_SYSTEMASTERISK, NULL, SND_ALIAS | SND_ALIAS_ID | SND_ASYNC | SND_SYSTEM);
	}
}

void show_context_menu(HWND wnd) {
	POINT p;
	GetCursorPos(&p);
	HMENU menu = CreatePopupMenu();

	if (menu) {
		InsertMenu(menu, -1, MF_BYPOSITION,
			rec_is_running() ? WM_SNV_REC_STOP : WM_SNV_REC_START,
			rec_is_running() ? "Stop" : "Capture");
		InsertMenu(menu, -1, MF_BYPOSITION, WM_SNV_REC_SETTINGS, "Settings...");
		InsertMenu(menu, -1, MF_BYPOSITION, WM_SNV_QUIT, "Quit");

		SetForegroundWindow(wnd);
		TrackPopupMenu(menu, TPM_BOTTOMALIGN, p.x, p.y, 0, wnd, NULL);
		DestroyMenu(menu);
	}
}

LRESULT CALLBACK wndproc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam) {
	if (msg == WM_APP && (lParam == WM_RBUTTONDOWN || lParam == WM_CONTEXTMENU)) {
		show_context_menu(hwnd);
	}

	else if (msg == WM_COMMAND) {
		if (LOWORD(wParam) == WM_SNV_QUIT) {
			ni_callback();
		}

		else if (LOWORD(wParam) == WM_SNV_REC_START) {
			rec_start();
		}

		else if (LOWORD(wParam) == WM_SNV_REC_STOP) {
			rec_stop();
		}

		else if (LOWORD(wParam) == WM_SNV_REC_SETTINGS) {
			rec_set_settings_window_flag(1);
		}
	}

	else if (msg == WM_TASKBARCREATED) {
		setup_icon(hwnd);
	}

	return DefWindowProc(hwnd, msg, wParam, lParam);
}

DWORD WINAPI wndthread(LPVOID param) {
	const char wndclass[] = "snvclNotifyIconHandlerWnd";
	WNDCLASSEX wc = { 0 };
	HMODULE inst = GetModuleHandle(NULL);

	wc.cbSize = sizeof(WNDCLASSEX);
	wc.lpfnWndProc = wndproc;
	wc.lpszClassName = wndclass;
	wc.hInstance = inst;

	RegisterClassEx(&wc);
	ni_wnd = CreateWindowEx(0, wndclass, "You don't see this", 0, 0, 0, 0, 0, NULL, NULL, inst, NULL);
	setup_icon(ni_wnd);

	MSG msg;
	while (GetMessage(&msg, NULL, 0, 0) > 0) {
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}

	return 0;
}

void snv_setup_client_icon(void(*callback)()) {
	id_mod = time(0);
	ni_callback = callback;
	WM_TASKBARCREATED = RegisterWindowMessage(TEXT("TaskbarCreated"));
	CreateThread(NULL, 0, wndthread, NULL, 0, NULL);
}

void snv_remove_client_icon() {
	NOTIFYICONDATA ni;

	memset(&ni, 0, sizeof(ni));
	ni.cbSize = sizeof(ni);
	ni.guidItem = ICONID;
	ICONID_PATCH(ni.guidItem);
	ni.uFlags = NIF_GUID;

	Shell_NotifyIcon(NIM_DELETE, &ni);
	DestroyWindow(ni_wnd);
}
