#pragma once

#include "types.h"
#include <math.h>

static inline struct pvec scale(struct pvec v, double s) {
    return (struct pvec){v.x * s, v.y * s};
}

static inline struct pvec diff(struct pvec a, struct pvec b) {
    return (struct pvec){a.x - b.x, a.y - b.y};
}

static inline struct pvec sum(struct pvec a, struct pvec b) {
    return (struct pvec){b.x + a.x, b.y + a.y};
}

static inline double dot(struct pvec a, struct pvec b) {
    return a.x * b.x + a.y * b.y;
}

static inline double length(struct pvec v) {
    return hypot(v.x, v.y);
}

static inline double angle(struct pvec v) {
    return atan2(v.y, v.x);
}

static inline struct pvec scs(double len, double angle) {
    return (struct pvec){len * cos(angle), len * sin(angle)};
}

static inline struct pvec normalize(struct pvec v) {
    return scale(v, 1 / length(v));
}