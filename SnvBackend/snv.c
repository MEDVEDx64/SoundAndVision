#define WIN32_LEAN_AND_MEAN

#include <stdlib.h>
#include <Windows.h>
#include <WinSock2.h>
#include <libavcodec\avcodec.h>
#include <libavformat\avformat.h>

#include "snv.h"

int snv_init() {
	WSADATA data;
	if (WSAStartup(MAKEWORD(2, 2), &data)) {
		return 1;
	}

	av_register_all();
	avcodec_register_all();
	return 0;
}

void snv_quit() {
	WSACleanup();
}

typedef struct {
	int wide;
	const void *msg;
	const void *cap;
	int flags;
} mboxargs;

DWORD WINAPI mbthread(LPVOID p) {
	mboxargs *args = (mboxargs*)p;
	if (args->wide) {
		MessageBoxW(NULL, (LPCWSTR)args->msg, (LPCWSTR)args->cap, args->flags);
	}
	else {
		MessageBoxA(NULL, (const char*)args->msg, (const char*)args->cap, args->flags);
	}

	free(args);
	return 0;
}

void snv_messagebox(int wide, const void *msg, const void *cap, int mbflags) {
	mboxargs *args = malloc(sizeof(mboxargs));
	args->msg = msg;
	args->cap = cap;
	args->flags = mbflags;

	CreateThread(NULL, 0, mbthread, args, 0, NULL);
}

#undef WIN32_LEAN_AND_MEAN
