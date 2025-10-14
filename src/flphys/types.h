#ifndef LIBFLPHYS_TYPES_H
#define LIBFLPHYS_TYPES_H

struct pvec {
    double x; // m | m/s | N
    double y; // m | m/s | N
};

struct pobj {
    struct pvec pos;   // m
    struct pvec mov;   // m/s
    double mass;       // kg
    double radius;     // m
    double area;       // m^2
    double volume;     // m^3
    struct pvec force; // N
};

struct phys {
    double density;               // ambient air density kg/m^3
    struct pvec accel_of_gravity; // constant acceleration acting on all objects m/s^2
    struct pvec wind;             // ambien wind m/s
    int is_gravity;               // inter-object gravity flag
    double time;                  // total simulation time
    int objects_num;              // number of objects in array
    struct pobj objects[];        // objects array
};

#endif
