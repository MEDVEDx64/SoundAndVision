#ifndef SCREEN_H
#define SCREEN_H

#include "snv.h"
#include "stage2.h"

#ifdef __cplusplus
extern "C" {
#endif

	SNVEXPORT int scr_count_displays();

	// Returns width and height in uint16 format, bundled together (w<<16|h), or 0 in case of error
	SNVEXPORT int scr_get_resolution(int display);

	// Returns non-zero on failure
	SNVEXPORT int scr_capture(s2_frame *frm, int display, unsigned int den);

#ifdef __cplusplus
}
#endif // __cplusplus

#endif
