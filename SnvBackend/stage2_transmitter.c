#define WIN32_LEAN_AND_MEAN

#include <stdio.h>
#include <stdlib.h>
#include <WinSock2.h>
#include <WS2tcpip.h>
#include <lz4.h>

#include "stage2.h"
#include "net.h"

int create_socket(s2_send_context *c, const char *addr, uint16_t port) {
	SOCKET cs = INVALID_SOCKET;
	struct addrinfo hints, *ai;

	ZeroMemory(&hints, sizeof(hints));
	hints.ai_family = AF_UNSPEC;
	hints.ai_socktype = SOCK_STREAM;
	hints.ai_protocol = IPPROTO_TCP;

	char portstr[6];
	sprintf(portstr, "%u\0", port);
	int r = getaddrinfo(addr, (PCSTR)(&portstr), &hints, &ai);

	if (r) {
		return 1;
	}

	struct addrinfo *p;
	for (p = ai; p != NULL; p = p->ai_next) {
		cs = socket(p->ai_family, p->ai_socktype, p->ai_protocol);
		if (cs == INVALID_SOCKET) {
			return 2;
		}

		if (connect(cs, p->ai_addr, (int)p->ai_addrlen) == SOCKET_ERROR) {
			closesocket(cs);
			continue;
		}

		break;
	}

	freeaddrinfo(ai);
	if (cs == INVALID_SOCKET) {
		return 3;
	}

	c->socket = cs;
	return 0;
}

int s2_send_init(s2_send_context *c, const char *addr, uint16_t port, int flags) {
	memset(c, 0, sizeof(s2_send_context));
	c->socket = (int)INVALID_SOCKET;
	return create_socket(c, addr, port);
}

void fill_picture(s2_send_context *c, void **pic, int *psize, s2_frame *frm) {
	const int w = frm->w, h = frm->h;
	int nsize = (int)(w * h * S2_PIXEL_SIZE);
	void *np = malloc(nsize);

	memcpy(np, frm->pixels, nsize);
	if (c->pprev == NULL) {
		c->pprev = malloc(nsize);
		memset(c->pprev, 0, nsize);
	}

	register uint8_t tmp = 0;
	register uint8_t *pixels = np;
	register uint8_t *prev = c->pprev;
	const void *stop = (void*)((int)np + nsize);

	while(pixels != stop) {
		tmp = *pixels;
		*pixels ^= *prev;
		*prev = tmp;
		++pixels;
		++prev;
	}

	int bsize = LZ4_compressBound(nsize);
	*pic = malloc(bsize);
	*psize = LZ4_compress_default((const char*)np, (char*)(*pic), nsize, bsize);

	free(np);
}

int s2_send(s2_send_context *c, s2_frame *frm) {
	if (!frm->w || !frm->h) {
		return 1;
	}

	if (frm->w != c->pw || frm->h != c->ph) {
		c->pw = frm->w;
		c->ph = frm->h;

		if (c->pprev) {
			free(c->pprev);
			c->pprev = NULL;
		}
	}

	void *pic = NULL;
	int psize = 0;
	fill_picture(c, &pic, &psize, frm);
	c->counter++;

	net_media_packet *npkt = malloc(NET_MEDIA_PKT_HEADER_SIZE + psize);
	memset(npkt, 0, NET_MEDIA_PKT_HEADER_SIZE);
	npkt->channel = NET_CHAN_VIDEO;
	npkt->size = psize;

	npkt->width = frm->w;
	npkt->height = frm->h;

	memcpy(npkt->payload, pic, psize);

	int r = send(
		c->socket,
		(const char*)npkt,
		NET_MEDIA_PKT_HEADER_SIZE + psize,
		0);

	if (r == SOCKET_ERROR) {
		return 1;
	}

	free(pic);
	free(npkt);

	return 0;
}

void s2_send_close(s2_send_context *c) {
	closesocket(c->socket);
	c->socket = 0;

	if (c->pprev != NULL) {
		free(c->pprev);
		c->pprev = NULL;
	}
}

#undef WIN32_LEAN_AND_MEAN
