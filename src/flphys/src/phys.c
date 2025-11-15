#include "phys.h"

#include <math.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>

#include "types.h"
#include "vec_ops.h"

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
    *pvec = scs(len, angle);
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
    return length(*pvec);
}

double pvec_get_angle(const struct pvec *pvec) {
    return angle(*pvec);
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

double pobj_get_bounce(const struct pobj *pobj) {
    return pobj->bounce;
}

void pobj_set_bounce(struct pobj *pobj, double bounce) {
    pobj->bounce = bounce;
}

unsigned char *pobj_ref_color(struct pobj *pobj) {
    return pobj->color;
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

struct phys *phys_create(int objects_num) {
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

struct pobj *phys_ref_object(struct phys *phys, int id) {
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

static void compute_collision(struct phys *phys) {
    for (int i = 0; i < phys->objects_num - 1; i++) {
        for (int j = i + 1; j < phys->objects_num; j++) {

            struct pobj *obj1 = &phys->objects[i];
            struct pobj *obj2 = &phys->objects[j];

            struct pvec delta_pos = diff(obj2->pos, obj1->pos);
            double dist = length(delta_pos);
            double penetration = obj1->radius + obj2->radius - dist;

            if (penetration > 0) {
                struct pvec N = normalize(delta_pos);
                obj1->pos =
                    sum(obj1->pos, scale(N, -penetration * obj2->mass / (obj1->mass + obj2->mass)));

                obj2->pos =
                    sum(obj2->pos, scale(N, penetration * obj1->mass / (obj1->mass + obj2->mass)));

                double v1n = dot(obj1->mov, N);
                double v2n = dot(obj2->mov, N);

                double vr = v1n - v2n;

                double m1 = obj1->mass;
                double m2 = obj2->mass;

                double e = obj1->bounce * obj2->bounce;

                double v1nAfter = v1n - (1 + e) * (m2 / (m1 + m2)) * vr;
                double v2nAfter = v2n + (1 + e) * (m1 / (m1 + m2)) * vr;

                obj1->mov = sum(obj1->mov, scale(N, v1nAfter - v1n));
                obj2->mov = sum(obj2->mov, scale(N, v2nAfter - v2n));
            }
        }
    }
}

static int pobj_run(struct pobj *obj, double time) {
    if (obj->mass == 0) {
        return PHYS_RES_ERR_ZERO_MASS;
    }
    struct pvec accel = scale(obj->force, 1.0 / obj->mass);
    obj->pos = sum(obj->pos, scale(sum(obj->mov, scale(accel, time * 0.5)), time));
    obj->mov = sum(obj->mov, scale(accel, time));
    return PHYS_RES_OK;
}

static void compute_object_force(const struct phys *phys, struct pobj *obj) {
    obj->force = scale(phys->accel_of_gravity, obj->mass);

    if (phys->density > 0) {
        struct pvec relative_mov = diff(obj->mov, phys->wind);
        double relative_speed_square = dot(relative_mov, relative_mov);
        if (relative_speed_square > 0) {
            double air_resistance_force =
                obj->area * phys->density * relative_speed_square * 0.5 * PHYS_BALL_DRAG_COEF;

            double k = air_resistance_force / sqrt(relative_speed_square);
            obj->force = diff(obj->force, scale(relative_mov, k));
        }
        obj->force = diff(obj->force, scale(phys->accel_of_gravity, obj->volume * phys->density));
    }
}

static int compute_gravity(struct phys *phys) {
    for (int i = 0; i < phys->objects_num - 1; i++) {
        for (int j = i + 1; j < phys->objects_num; j++) {
            struct pobj *obj_a = &phys->objects[i];
            struct pobj *obj_b = &phys->objects[j];

            struct pvec dist_v = diff(obj_a->pos, obj_b->pos);
            double distance_square = dot(dist_v, dist_v);

            if (distance_square == 0) {
                return PHYS_RES_ERR_ZERO_DIST;
            }

            double gravity_force = PHYS_G * obj_a->mass * obj_b->mass / distance_square;

            double k = gravity_force / sqrt(distance_square);

            obj_a->force = diff(obj_a->force, scale(dist_v, k));
            obj_b->force = sum(obj_b->force, scale(dist_v, k));
        }
    }
    return PHYS_RES_OK;
}

int phys_run(struct phys *phys, double step_time, long steps) {
    int err;
    for (int i = 0; i < steps; i++) {
        for (int i = 0; i < phys->objects_num; i++) {
            compute_object_force(phys, &phys->objects[i]);
        }
        if (phys->is_gravity && phys->objects_num > 1) {
            if ((err = compute_gravity(phys)) != PHYS_RES_OK) {
                return err;
            }
        }
        for (int i = 0; i < phys->objects_num; i++) {
            if ((err = pobj_run(&phys->objects[i], step_time)) != PHYS_RES_OK) {
                return err;
            }
        }
        compute_collision(phys);
    }
    phys->time += step_time * steps;
    return PHYS_RES_OK;
}

int phys_run_bench(struct phys *phys, double step_time, long steps, double *execution_time) {
    clock_t start = clock();
    int err = phys_run(phys, step_time, steps);
    *execution_time = (double)(clock() - start) / CLOCKS_PER_SEC;
    return err;
}
