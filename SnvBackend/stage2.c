/* Generic video-related tools */

#include <stdlib.h>

#include "stage2.h"

void s2_alloc_frame(s2_frame *frm, uint16_t w, uint16_t h) {
	frm->w = w;
	frm->h = h;
	frm->error = 0;
	frm->flags = 0;
	frm->pixels = malloc(w * h * S2_PIXEL_SIZE + w * 2);

	if (!frm->pixels)
		frm->error = 1;
}

void s2_free_frame(s2_frame *frm) {
	if (!frm->error)
		free(frm->pixels);
}
