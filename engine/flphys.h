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

// Constants
#define PHYS_G 6.6743015151515151514e-11
#define PHYS_PI 3.1415926535897932385
#define PHYS_AIR_DENSITY 1.225
#define PHYS_ACCEL_OF_FREE_FALL 9.80665
#define PHYS_BALL_DRAG_COEF 0.47

// Results
#define PHYS_RES_OK 0
#define PHYS_RES_ERR_NULL_PTR -1
#define PHYS_RES_ERR_ZERO_DIST -2
#define PHYS_RES_ERR_ZERO_MASS -3

const char *phys_strerror(int result);

// Vector
struct pvec;
FLPHYS_EXPORT void pvec_set_scs(struct pvec *pvec, double len, double angle);
FLPHYS_EXPORT double pvec_get_x(const struct pvec *pvec);
FLPHYS_EXPORT double pvec_get_y(const struct pvec *pvec);
FLPHYS_EXPORT void pvec_set_x(struct pvec *pvec, double x);
FLPHYS_EXPORT void pvec_set_y(struct pvec *pvec, double y);
FLPHYS_EXPORT double pvec_get_len(const struct pvec *pvec);
FLPHYS_EXPORT double pvec_get_angle(const struct pvec *pvec);

// Object
struct pobj;
FLPHYS_EXPORT struct pvec *pobj_ref_pos(struct pobj *pobj);
FLPHYS_EXPORT struct pvec *pobj_ref_mov(struct pobj *pobj);
FLPHYS_EXPORT double pobj_get_mass(const struct pobj *pobj);
FLPHYS_EXPORT void pobj_set_mass(struct pobj *pobj, double mass);
FLPHYS_EXPORT double pobj_get_radius(const struct pobj *pobj);
FLPHYS_EXPORT void pobj_set_radius(struct pobj *obj, double radius);

// Phys
struct phys;
FLPHYS_EXPORT /*maybe null*/ struct phys *phys_create(int objects_num);
FLPHYS_EXPORT double phys_get_density(const struct phys *phys);
FLPHYS_EXPORT void phys_set_density(struct phys *phys, double density);
FLPHYS_EXPORT struct pvec *phys_ref_accel_of_gravity(struct phys *phys);
FLPHYS_EXPORT struct pvec *phys_ref_wind(struct phys *phys);
FLPHYS_EXPORT /*maybe null*/ struct pobj *phys_ref_object(struct phys *phys, int id);
FLPHYS_EXPORT int phys_get_objects_num(const struct phys *phys);
FLPHYS_EXPORT int phys_get_is_gravity(const struct phys *phys);
FLPHYS_EXPORT void phys_set_is_gravity(struct phys *phys, int is_gravity);
FLPHYS_EXPORT double phys_get_time(const struct phys *phys);
FLPHYS_EXPORT int phys_run(struct phys *phys, double step_time, int steps);
FLPHYS_EXPORT void phys_destroy(struct phys *phys);

#ifdef __cplusplus
}
#endif

#endif
