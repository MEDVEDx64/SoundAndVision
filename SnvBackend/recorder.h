#ifndef RECORDER_H
#define RECORDER_H

#include "snv.h"
#include "stage2.h"

typedef struct {
	char prefix[0x200];
	uint32_t split_max_megabytes;
	uint32_t split_max_count;
} rec_settings;

typedef struct {
	int is_recording;
	AVCodecContext *encctx;
	AVFormatContext *fctx;
	uint64_t bytes_written;
	uint32_t chunks_written;
	uint64_t pts;
	uint64_t sync;
} rec_context;

#ifdef __cplusplus
extern "C" {
#endif // __cplusplus

	SNVEXPORT int rec_is_running();
	SNVEXPORT int rec_get_sessid();
	SNVEXPORT int rec_get_settings_window_flag();
	SNVEXPORT void rec_set_settings_window_flag(int value);

	SNVEXPORT void rec_start();
	SNVEXPORT void rec_stop();

	SNVEXPORT void rec_commit_frame(rec_context *ctx, s2_frame *frm, int display);

#ifdef __cplusplus
}
#endif // __cplusplus

#endif // !RECORDER_H
