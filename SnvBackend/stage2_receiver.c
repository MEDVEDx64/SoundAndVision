#define WIN32_LEAN_AND_MEAN

#include <stdlib.h>
#include <WinSock2.h>
#include <WS2tcpip.h>
#include <time.h>
#include <lz4.h>

#include "stage2.h"
#include "net.h"

SOCKET listen_socket(uint16_t port, int *terminate) {
	struct addrinfo hints, *ai = NULL;
	ZeroMemory(&hints, sizeof(hints));
	hints.ai_family = AF_INET;
	hints.ai_socktype = SOCK_STREAM;
	hints.ai_protocol = IPPROTO_TCP;
	hints.ai_flags = AI_PASSIVE;

	char portstr[6];
	sprintf(portstr, "%u\0", port);
	if (getaddrinfo(NULL, (PCSTR)(&portstr), &hints, &ai)) {
		freeaddrinfo(ai);
		return INVALID_SOCKET;
	}

	SOCKET ls = socket(ai->ai_family, ai->ai_socktype, ai->ai_protocol);
	if (ls == INVALID_SOCKET) {
		freeaddrinfo(ai);
		return INVALID_SOCKET;
	}

	if (bind(ls, ai->ai_addr, (int)ai->ai_addrlen) == SOCKET_ERROR) {
		freeaddrinfo(ai);
		closesocket(ls);
		return INVALID_SOCKET;
	}

	freeaddrinfo(ai);
	if (listen(ls, SOMAXCONN) == SOCKET_ERROR) {
		freeaddrinfo(ai);
		closesocket(ls);
		return INVALID_SOCKET;
	}

	fd_set rd;
	FD_ZERO(&rd);
	FD_SET(ls, &rd);
	struct timeval timeout;
	timeout.tv_usec = 0;
	timeout.tv_sec = 1;

	while(!(*terminate)) {
		if (select(ls, &rd, NULL, NULL, &timeout)) {
			SOCKET s = accept(ls, NULL, NULL);
			closesocket(ls);
			return s;
		}
	}

	closesocket(ls);
	return INVALID_SOCKET;
}

int recv_all(SOCKET s, void *d, int size, int *terminate) {
	int left = size;
	while (left) {
		if (*terminate) {
			return 1;
		}

		int r = recv(s, (char*)((int)d + (size - left)), left, 0);
		if (r <= 0) {
			return 1;
		}

		left -= r;
	}

	return 0;
}

int s2_recv_thread(s2_recv_context *rctx, uint16_t port, int flags, int(*recv_callback)(s2_frame*)) {
	rctx->counter = 0;
	rctx->terminate = 0;
	
	SOCKET socket = listen_socket(port, &rctx->terminate);
	if (socket == INVALID_SOCKET) {
		return !rctx->terminate;
	}

	net_media_packet *buf = malloc(NET_MEDIA_PKT_HEADER_SIZE);
	void *pktdata = malloc(0);
	uint8_t *prev = NULL;
	uint16_t pw = 0;
	uint16_t ph = 0;
	unsigned int pdsize = 0;

	while (!rctx->terminate) {
		if (recv_all(socket, buf, NET_MEDIA_PKT_HEADER_SIZE, &rctx->terminate)) {
			break;
		}

		if (buf->channel != NET_CHAN_VIDEO) {
			continue;
		}

		if (buf->size > pdsize) {
			pdsize = buf->size;
			pktdata = realloc(pktdata, pdsize);
		}

		if (recv_all(socket, pktdata, buf->size, &rctx->terminate)) {
			break;
		}

		rctx->counter++;

		int psize = buf->width * buf->height * S2_PIXEL_SIZE;
		void *pic = malloc(psize);
		if (LZ4_decompress_safe((const char*)pktdata, (char*)pic, buf->size, psize) < 0) {
			free(pic);
			break;
		}

		if (buf->width != pw || buf->height != ph) {
			pw = buf->width;
			ph = buf->height;

			if (prev) {
				free(prev);
				prev = NULL;
			}
		}

		if (prev == NULL) {
			prev = malloc(psize);
			memset(prev, 0, psize);
		}
		
		register uint8_t *d = pic;
		register uint8_t *s = prev;
		const void *stop = (void*)((int)pic + psize);

		while (d != stop) {
			*d ^= *s;
			*s = *d;
			++d;
			++s;
		}

		s2_frame rf;
		s2_alloc_frame(&rf, buf->width, buf->height);
		memcpy(rf.pixels, pic, psize);
		free(pic);

		if (recv_callback(&rf)) {
			break;
		}
	}

	closesocket(socket);
	free(pktdata);
	free(buf);

	if (prev != NULL) {
		free(prev);
	}

	return 0;
}

#undef WIN32_LEAN_AND_MEAN
