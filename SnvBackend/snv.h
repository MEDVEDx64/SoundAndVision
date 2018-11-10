#ifndef SNV_H
#define SNV_H

#include <stdint.h>

#define SNVEXPORT __declspec(dllexport)

#ifdef __cplusplus
extern "C" {
#endif

	SNVEXPORT int snv_init();
	SNVEXPORT void snv_quit();
	SNVEXPORT void snv_setup_client_icon(void(*callback)());
	SNVEXPORT void snv_show_notification(const wchar_t *text, int beep);
	SNVEXPORT void snv_remove_client_icon();

	void snv_messagebox(int wide, const void *msg, const void *cap, int mbflags); // Non-blocking messagebox

#ifdef __cplusplus
}
#endif // __cplusplus

#endif
