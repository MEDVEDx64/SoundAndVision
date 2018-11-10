#ifndef NET_H
#define NET_H

// The Protocol Stage 2 packets

#include "snv.h"

#define NET_CHAN_NOTHING 0
#define NET_CHAN_VIDEO 0x48323634

#define NET_MEDIA_PKT_HEADER_SIZE 16

#pragma pack(push,1)

typedef struct {
	int32_t channel;
	int32_t flags;
	uint32_t size;
	uint16_t width;
	uint16_t height;
	uint8_t payload[0];
} net_media_packet;

#pragma pack(pop)

#endif
