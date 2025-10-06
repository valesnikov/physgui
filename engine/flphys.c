#include "flphys.h"
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

struct pvec {
    double x; // m | m/s | N
    double y; // m | m/s | N
};

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

struct pobj {
    struct pvec pos;   // m
    struct pvec mov;   // m/s
    double mass;       // kg
    double radius;     // m
    double area;       // m^2
    double volume;     // m^3
    struct pvec force; // N
};

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

    obj->pos.x += (obj->mov.x + acceleration.x * time * 0.5L) * time;
    obj->pos.y += (obj->mov.y + acceleration.y * time * 0.5L) * time;

    obj->mov.x += acceleration.x * time;
    obj->mov.y += acceleration.y * time;
    return PHYS_RES_OK;
}

struct phys {
    double density;               // ambient air density kg/m^3
    struct pvec accel_of_gravity; // constant acceleration acting on all objects m/s^2
    struct pvec wind;             // ambien wind m/s
    int is_gravity;               // inter-object gravity flag
    double time;                  // total simulation time
    unsigned int objects_num;     // number of objects in array
    struct pobj objects[];        // objects array
};

struct phys *phys_create(unsigned int objects_num) {
    const size_t size = sizeof(struct phys) + objects_num * sizeof(struct pobj);
    struct phys *phys = malloc(size);
    memset(phys, 0, size);
    if (phys != NULL) {
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

int phys_get_objects_num(const struct phys *phys) {
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

static int compute_objects_force(const struct phys *phys, struct pobj *obj) {
    obj->force = (struct pvec){0};

    const double real_mass = obj->mass - (obj->volume * phys->density);

    struct pvec real_obj_mov = {
        obj->mov.x - phys->wind.x,
        obj->mov.y - phys->wind.y,
    };

    const double real_obj_speed = pvec_get_len(&real_obj_mov);

    if (real_obj_speed != 0) {
        const double air_f =
            obj->area * phys->density * real_obj_speed * real_obj_speed * 0.5 * PHYS_BALL_DRAG_COEF;

        const double k = air_f / real_obj_speed;
        obj->force.x -= real_obj_mov.x * k;
        obj->force.y -= real_obj_mov.y * k;
    }

    obj->force.x += phys->accel_of_gravity.x * real_mass;
    obj->force.y += phys->accel_of_gravity.y * real_mass;

    if (phys->is_gravity && phys->objects_num > 1) {
        for (unsigned int i = 0; i < phys->objects_num; i++) {
            const struct pobj *other_obj = phys->objects + i;
            if (other_obj != obj) {

                const struct pvec dist = {
                    .x = obj->pos.x - other_obj->pos.x,
                    .y = obj->pos.y - other_obj->pos.y,
                };

                const double dist_len = pvec_get_len(&dist);
                if (dist_len == 0) {
                    return PHYS_RES_ERR_ZERO_DIST;
                }
                const double gravy_f = real_mass *
                                       (other_obj->mass - (other_obj->volume * phys->density)) *
                                       PHYS_G / (dist_len * dist_len);

                const double k = gravy_f / dist_len;
                obj->force.x -= dist.x * k;
                obj->force.y -= dist.y * k;
            }
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
            err = compute_objects_force(phys, phys->objects + i);
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
