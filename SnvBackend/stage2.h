#ifndef STAGE2_H
#define STAGE2_H

#include <stdio.h>
#include <Windows.h>
#include <libavcodec\avcodec.h>
#include <libavformat\avformat.h>

#include "snv.h"

// The pixel format is always RGB
#define S2_PIXEL_SIZE 3

typedef struct {
	uint16_t w;
	uint16_t h;
	void *pixels;
	int error;
	int flags;
} s2_frame;

typedef struct {
	uint64_t counter; // Decoded frames
	int terminate; // Should the reciever thread exit
} s2_recv_context;

typedef struct {
	uint64_t counter;
	int socket;
	void *pprev;
	uint16_t pw;
	uint16_t ph;
} s2_send_context;

#ifdef __cplusplus
extern "C" {
#endif // __cplusplus

	SNVEXPORT void s2_alloc_frame(s2_frame *frm, uint16_t w, uint16_t h);
	SNVEXPORT void s2_free_frame(s2_frame *frm);

	SNVEXPORT int s2_recv_thread(
		s2_recv_context *rctx,
		uint16_t port,
		int flags,
		int(*recv_callback)(s2_frame*)); // Thread exits when the callback returns non-zero

	SNVEXPORT int s2_send_init(s2_send_context *c, const char *addr, uint16_t port, int flags);
	SNVEXPORT int s2_send(s2_send_context *sctx, s2_frame *frm);

	SNVEXPORT void s2_send_close(s2_send_context *sctx);

#ifdef __cplusplus
}
#endif // __cplusplus

#endif
