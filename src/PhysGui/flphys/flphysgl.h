#ifndef LIBFLPHYS_H
#define LIBFLPHYS_H

#include <stdbool.h>
#include <stddef.h>
#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

#ifndef FLPHYS_EXPORT
#define FLPHYS_EXPORT
#endif

FLPHYS_EXPORT struct physgl *physgl_init(unsigned int circle_verts /*at least 3*/);

FLPHYS_EXPORT void physgl_on_resize(struct physgl *phgl, int width, int height);

FLPHYS_EXPORT void physgl_preview_render(struct physgl *phgl, double center_x, double center_y, double scale);

FLPHYS_EXPORT void physgl_destroy(struct physgl *phgl);

#ifdef __cplusplus
}
#endif

#endif
