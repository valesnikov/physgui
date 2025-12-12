#pragma once

#include <stdbool.h>
#include <stddef.h>
#include <stdint.h>

#include "phys.h"

#ifdef __cplusplus
extern "C" {
#endif

#ifndef FLPHYS_EXPORT
#define FLPHYS_EXPORT
#endif

// Create static background builder
FLPHYS_EXPORT struct backgl_builder *backgl_builder_create(void);

// Add triangle element to builder
FLPHYS_EXPORT void backgl_builder_set_background_color(
    struct backgl_builder *bglb,
    unsigned char r,
    unsigned char g,
    unsigned char b
);

FLPHYS_EXPORT void backgl_builder_add(
    struct backgl_builder *bglb,
    float x1,
    float y1,
    float x2,
    float y2,
    float x3,
    float y3,
    unsigned char r,
    unsigned char g,
    unsigned char b
);

// Load to OpenGL and free builder
FLPHYS_EXPORT struct backgl *backgl_builder_build(struct backgl_builder *bglb);

FLPHYS_EXPORT void backgl_builder_cancel(struct backgl_builder *bglb);

FLPHYS_EXPORT void
backgl_render(struct backgl *bgl, double center_x, double center_y, double scale, double aspect);

#ifdef __cplusplus
}
#endif
