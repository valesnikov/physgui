#pragma once

#include <stdbool.h>
#include <stddef.h>
#include <stdint.h>

#include "../phys/phys.h"

#ifdef __cplusplus
extern "C" {
#endif

#ifndef FLPHYS_EXPORT
#define FLPHYS_EXPORT
#endif

FLPHYS_EXPORT struct physgl *physgl_create(struct phys *phys);

FLPHYS_EXPORT void physgl_update(struct physgl *phgl);

FLPHYS_EXPORT void physgl_on_resize(struct physgl *phgl, double aspect_ratio);

FLPHYS_EXPORT void physgl_render(struct physgl *phgl, double center_x, double center_y, double scale, double aspect);

FLPHYS_EXPORT void physgl_destroy(struct physgl *phgl);

#ifdef __cplusplus
}
#endif
