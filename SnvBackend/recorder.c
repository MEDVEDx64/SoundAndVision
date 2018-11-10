#include "recorder.h"
#include "snv.h"

#include <stdio.h>
#include <string.h>
#include <time.h>

#include <libavcodec\avcodec.h>
#include <libavformat\avformat.h>
#include <libswscale\swscale.h>
#include <libavutil\opt.h>

#include <Windows.h>

#define CAP_PREFIX "Capture"
#define _1M 1048576

static int is_recording = 0;
static int is_frames_been_committed = 0;
static rec_settings settings;
static int settings_window_flag = 0;
static int token = 0;

int rec_get_sessid() {
	return token;
}

int rec_get_settings_window_flag() {
	return settings_window_flag;
}

void rec_set_settings_window_flag(int value) {
	settings_window_flag = !!value;
}

int read_settings_from_chest(const char *fn, rec_settings *dst) {
	FILE *f = fopen(fn, "r");
	if (f <= 0) {
		return 1;
	}

	char line[0x1400];
	char param[0x400];
	char arg[0x1000];

	while (fgets(line, sizeof(line), f) != NULL) {
		if (!line[0] || line[0] == ' ') {
			continue;
		}

		int split = 0;
		int count = 0;
		char *str = line;
		while (*str) {
			if (*str == ' ') {
				memcpy(param, line, count);
				param[count] = 0;
				str++;
				strcpy(arg, str);
				split = 1;
				break;
			}

			count++;
			str++;
			if (!*str) {
				break;
			}
		}

		if (!split) {
			continue;
		}

		register int i;
		const int arg_len = sizeof(arg);
		for (i = 0; i < arg_len; ++i) {
			if (!arg[i]) {
				break;
			}

			if (arg[i] == '\n') {
				arg[i] = 0;
				break;
			}
		}

		if (!strcmp(param, "RecPrefix")) {
			if (strlen(arg) < sizeof(dst->prefix)) {
				strcpy(dst->prefix, arg);
			}
		}

		if (!strcmp(param, "RecSplitMaxMegabytes")) {
			int i = atoi(arg);
			if (i > 0) {
				dst->split_max_megabytes = i;
			}
		}

		else if (!strcmp(param, "RecSplitMaxCount")) {
			int i = atoi(arg);
			if (i > 0) {
				dst->split_max_count = i;
			}
		}
	}

	fclose(f);
	return 0;
}

void create_default_settings(rec_settings *dst) {
	memset(dst, 0, sizeof(rec_settings));
	dst->split_max_megabytes = 1024;
}

void create_file_name(char *dst, char *dst_dir, int display, const char *ext) {
	time_t t = time(NULL);
	struct tm tm = *localtime(&t);

	char ds[16];
	char ts[16];
	sprintf(ds, "%d-%02d-%02d", tm.tm_year + 1900, tm.tm_mon + 1, tm.tm_mday);
	sprintf(ts, "%02d-%02d-%02d_%d", tm.tm_hour, tm.tm_min, tm.tm_sec, display);
	sprintf(dst, "%s\\%s\\%s_%s.%s", (strlen(settings.prefix) ? settings.prefix : CAP_PREFIX), ds, ds, ts, ext);
	sprintf(dst_dir, "%s\\%s", (strlen(settings.prefix) ? settings.prefix : CAP_PREFIX), ds);
}

uint64_t get_timestamp() {
	FILETIME ft;
	GetSystemTimeAsFileTime(&ft);
	return ((uint64_t)ft.dwHighDateTime << 32) | (uint64_t)ft.dwLowDateTime;
}

int rec_is_running() {
	return is_recording & is_frames_been_committed;
}

void rec_start() {
	token = (int)clock();
	create_default_settings(&settings);
	read_settings_from_chest("Massacre.Snv.Client.txt", &settings);
	is_frames_been_committed = 0;
	is_recording = 1;
}

void rec_stop() {
	is_recording = 0;
}

int start(rec_context *ctx, uint16_t w, uint16_t h, int display) {
	ctx->fctx = avformat_alloc_context();
	AVCodec *vcodec = avcodec_find_encoder(AV_CODEC_ID_H264);
	AVOutputFormat *format = av_guess_format("avi", NULL, NULL);
	ctx->fctx->oformat = format;
	AVStream *vstream = avformat_new_stream(ctx->fctx, vcodec);

	vstream->codec->width = w;
	vstream->codec->height = h;

	ctx->encctx = avcodec_alloc_context3(vcodec);
	ctx->encctx->width = w;
	ctx->encctx->height = h;
	ctx->encctx->pix_fmt = AV_PIX_FMT_YUV420P;
	ctx->encctx->gop_size = 8;
	ctx->encctx->max_b_frames = 1;

	ctx->encctx->time_base.den = 1;
	ctx->encctx->time_base.num = 1;
	ctx->encctx->ticks_per_frame = 2;

	av_opt_set(ctx->encctx->priv_data, "preset", "superfast", 0);
	int err = avcodec_open2(ctx->encctx, vcodec, NULL);
	if (err < 0) {
		avformat_free_context(ctx->fctx);
		av_free(ctx->encctx);
		return 1;
	}

	char fn[0x300];
	char dir[0x300];
	create_file_name(fn, dir, display, "avi");
	if (!strlen(settings.prefix)) {
		CreateDirectory(CAP_PREFIX, NULL);
	}

	CreateDirectory(dir, NULL);

	avio_open(&ctx->fctx->pb, fn, AVIO_FLAG_WRITE);
	if (!ctx->fctx->pb) {
		avformat_free_context(ctx->fctx);
		avcodec_close(ctx->encctx);
		av_free(ctx->encctx);

		snv_messagebox(1, "Unable to write file. Ensure the settings are correct, and the destination path exist.", "Error", MB_ICONERROR);
		return 2;
	}

	avformat_write_header(ctx->fctx, NULL);
	ctx->bytes_written = 0;
	ctx->pts = 0;
	ctx->sync = get_timestamp();

	return 0;
}

#define START() \
if (start(ctx, frm->w, frm->h, display)) { \
	is_recording = 0; \
	return; \
}

void stop(rec_context *ctx) {
	av_write_trailer(ctx->fctx);
	avio_close(ctx->fctx->pb);
	avformat_free_context(ctx->fctx);
	avcodec_close(ctx->encctx);
	av_free(ctx->encctx);
}

void rec_commit_frame(rec_context *ctx, s2_frame *frm, int display) {
	if (is_recording != (!!ctx->is_recording)) {
		if (is_recording) {
			ctx->chunks_written = 0;
			START();
		}
		else {
			stop(ctx);
		}

		ctx->is_recording = is_recording;
	}

	if (!ctx->is_recording) {
		return;
	}

	if ((settings.split_max_megabytes && ctx->bytes_written / _1M >= settings.split_max_megabytes * 0.815)
		|| frm->w != ctx->encctx->width || frm->h != ctx->encctx->height) {
		ctx->chunks_written++;
		if (settings.split_max_count && ctx->chunks_written >= settings.split_max_count) {
			is_recording = 0;
			return;
		}

		stop(ctx);
		START();
	}

	is_frames_been_committed = 1;

	AVFrame *avfrm = av_frame_alloc();
	avfrm->width = frm->w;
	avfrm->height = frm->h;
	avfrm->format = AV_PIX_FMT_BGR24;
	avpicture_fill((AVPicture*)avfrm, NULL, avfrm->format, frm->w, frm->h);

	const int plane_size = frm->w * frm->h;
	const int plane_size_q = plane_size / 4;

	avfrm->data[0] = frm->pixels;

	AVFrame *avfrm2 = av_frame_alloc();
	avfrm2->width = frm->w;
	avfrm2->height = frm->h;
	avfrm2->format = ctx->encctx->pix_fmt;
	avpicture_fill((AVPicture*)avfrm2, NULL, avfrm2->format, frm->w, frm->h);

	avfrm2->data[0] = malloc(plane_size);
	avfrm2->data[1] = malloc(plane_size_q);
	avfrm2->data[2] = malloc(plane_size_q);

	struct SwsContext *swsctx = sws_getContext(frm->w, frm->h, avfrm->format, frm->w, frm->h, avfrm2->format, SWS_BICUBIC, NULL, NULL, NULL);
	sws_scale(swsctx, (const uint8_t *const *)avfrm->data, avfrm->linesize, 0, frm->h, avfrm2->data, avfrm2->linesize);
	sws_freeContext(swsctx);

	av_frame_free(&avfrm);

	AVPacket pkt;
	memset(&pkt, 0, sizeof(pkt));
	int got_pkt = 0;
	avcodec_encode_video2(ctx->encctx, &pkt, avfrm2, &got_pkt);

	free(avfrm2->data[0]);
	free(avfrm2->data[1]);
	free(avfrm2->data[2]);

	av_frame_free(&avfrm2);

	if (got_pkt == 1) {
		uint64_t ts = get_timestamp();
		ctx->pts += ((uint64_t)(ts - ctx->sync) / 17000ULL);
		pkt.pts = ctx->pts;
		ctx->sync = ts;

		av_write_frame(ctx->fctx, &pkt);
		ctx->bytes_written += pkt.size;
		av_packet_unref(&pkt);
	}
}
