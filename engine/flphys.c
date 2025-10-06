#include "flphys.h"
#include "types.h"

#include <math.h>
#include <stdlib.h>
#include <string.h>

const char *phys_strerror(int result) {
    switch (result) {
    case PHYS_RES_OK:
        return "Success";
    case PHYS_RES_ERR_NULL_PTR:
        return "Null pointer error";
    case PHYS_RES_ERR_ZERO_DIST:
        return "Zero distance error";
    case PHYS_RES_ERR_ZERO_MASS:
        return "Zero mass error";
    default:
        return "Unknown error code";
    }
}

void pvec_set_scs(struct pvec *pvec, double len, double angle) {
    pvec->x = len * cos(angle);
    pvec->y = len * sin(angle);
}

double pvec_get_x(const struct pvec *pvec) {
    return pvec->x;
}

double pvec_get_y(const struct pvec *pvec) {
    return pvec->y;
}

void pvec_set_x(struct pvec *pvec, double x) {
    pvec->x = x;
}

void pvec_set_y(struct pvec *pvec, double y) {
    pvec->y = y;
}

double pvec_get_len(const struct pvec *pvec) {
    return sqrt(pvec->x * pvec->x + pvec->y * pvec->y);
}

double pvec_get_angle(const struct pvec *pvec) {
    return atan2(pvec->y, pvec->x);
}

struct pvec *pobj_ref_pos(struct pobj *pobj) {
    return &pobj->pos;
}

struct pvec *pobj_ref_mov(struct pobj *pobj) {
    return &pobj->mov;
}

double pobj_get_mass(const struct pobj *pobj) {
    return pobj->mass;
}

void pobj_set_mass(struct pobj *pobj, double mass) {
    pobj->mass = mass;
}

double pobj_get_radius(const struct pobj *pobj) {
    return pobj->radius;
}

void pobj_set_radius(struct pobj *obj, double radius) {
    obj->radius = radius;
    obj->area = PHYS_PI * radius * radius;
    obj->volume = (4.0 / 3.0) * PHYS_PI * radius * radius * radius;
}

static int pobj_run(struct pobj *obj, double time) {
    if (obj->mass == 0) {
        return PHYS_RES_ERR_ZERO_MASS;
    }
    struct pvec acceleration = {
        .x = obj->force.x / obj->mass,
        .y = obj->force.y / obj->mass,
    };

    obj->pos.x += (obj->mov.x + acceleration.x * time * 0.5) * time;
    obj->pos.y += (obj->mov.y + acceleration.y * time * 0.5) * time;

    obj->mov.x += acceleration.x * time;
    obj->mov.y += acceleration.y * time;
    return PHYS_RES_OK;
}

struct phys *phys_create(unsigned int objects_num) {
    const size_t size = sizeof(struct phys) + objects_num * sizeof(struct pobj);
    struct phys *phys = malloc(size);
    if (phys != NULL) {
        memset(phys, 0, size);
        phys->density = 0;
        phys->accel_of_gravity = (struct pvec){.x = 0, .y = 0};
        phys->wind = (struct pvec){.x = 0, .y = 0};
        phys->objects_num = objects_num;
        phys->is_gravity = false;
        phys->time = 0;
    }
    return phys;
}

void phys_destroy(struct phys *phys) {
    free(phys);
}

double phys_get_density(const struct phys *phys) {
    return phys->density;
}

void phys_set_density(struct phys *phys, double density) {
    phys->density = density;
}

struct pvec *phys_ref_accel_of_gravity(struct phys *phys) {
    return &phys->accel_of_gravity;
}

struct pvec *phys_ref_wind(struct phys *phys) {
    return &phys->wind;
}

struct pobj *phys_ref_object(struct phys *phys, unsigned int id) {
    if (id >= phys->objects_num) {
        return NULL;
    }
    return &phys->objects[id];
}

unsigned int phys_get_objects_num(const struct phys *phys) {
    return phys->objects_num;
}

int phys_get_is_gravity(const struct phys *phys) {
    return phys->is_gravity;
}

void phys_set_is_gravity(struct phys *phys, int is_gravity) {
    phys->is_gravity = is_gravity;
}

double phys_get_time(const struct phys *phys) {
    return phys->time;
}

static void compute_object_force(const struct phys *phys, struct pobj *obj) {
    obj->force = (struct pvec){0};

    const struct pvec relative_mov = {
        obj->mov.x - phys->wind.x,
        obj->mov.y - phys->wind.y,
    };

    const double relative_speed = pvec_get_len(&relative_mov);

    if (phys->density != 0 && relative_speed != 0) {
        const double air_f =
            obj->area * phys->density * relative_speed * relative_speed * 0.5 * PHYS_BALL_DRAG_COEF;

        const double k = air_f / relative_speed;
        obj->force.x -= relative_mov.x * k;
        obj->force.y -= relative_mov.y * k;
    }

    const double relative_mass = obj->mass - (obj->volume * phys->density); // maybe negative
    obj->force.x += phys->accel_of_gravity.x * relative_mass;
    obj->force.y += phys->accel_of_gravity.y * relative_mass;
}

static int compute_gravity(struct phys *phys) {
    for (unsigned int i = 0; i < phys->objects_num - 1; i++) {
        for (unsigned int j = i + 1; j < phys->objects_num; j++) {
            struct pobj *a = &phys->objects[i];
            struct pobj *b = &phys->objects[j];

            const struct pvec dist = {
                .x = a->pos.x - b->pos.x,
                .y = a->pos.y - b->pos.y,
            };

            const double dist_len = pvec_get_len(&dist);
            if (dist_len == 0) {
                return PHYS_RES_ERR_ZERO_DIST;
            }

            const double gravy_f = a->mass * b->mass * PHYS_G / (dist_len * dist_len);

            const double k = gravy_f / dist_len;

            a->force.x -= dist.x * k;
            a->force.y -= dist.y * k;
            b->force.x += dist.x * k;
            b->force.y += dist.y * k;
        }
    }
    return PHYS_RES_OK;
}

int phys_run(struct phys *phys, double step_time, unsigned int steps) {
    if (phys->objects_num == 0)
        return PHYS_RES_OK;

    unsigned int counter = 0;
    int err;

    while (counter++ < steps) {
        for (unsigned int i = 0; i < phys->objects_num; i++) {
            compute_object_force(phys, phys->objects + i);
        }
        if (phys->is_gravity && phys->objects_num > 1) {
            err = compute_gravity(phys);
            if (err != PHYS_RES_OK)
                return err;
        }
        for (unsigned int i = 0; i < phys->objects_num; i++) {
            err = pobj_run(phys->objects + i, step_time);
            if (err != PHYS_RES_OK)
                return err;
        }
    }
    phys->time += step_time * steps;
    return PHYS_RES_OK;
}
