using System;
using System.Runtime.InteropServices;

namespace physgui
{
    public static partial class LibFlPhys
    {
        private const string LibraryName = "libflphys.so";

        public const double PHYS_G = 6.6743015151515151514e-11;
        public const double PHYS_PI = 3.1415926535897932385;
        public const double PHYS_AIR_DENSITY = 1.225;
        public const double PHYS_ACCEL_OF_FREE_FALL = 9.80665;
        public const double PHYS_BALL_DRAG_COEF = 0.47;

        public const int PHYS_RES_OK = 0;
        public const int PHYS_RES_ERR_NULL_PTR = -1;
        public const int PHYS_RES_ERR_ZERO_DIST = -2;
        public const int PHYS_RES_ERR_ZERO_MASS = -3;

        [LibraryImport(LibraryName)]
        public static partial int phys_get_is_gravity(IntPtr phys);

        [LibraryImport(LibraryName)]
        public static partial void phys_set_is_gravity(IntPtr phys, int isGravity);

        [LibraryImport(LibraryName)]
        public static partial IntPtr phys_create(uint objects_num);

        [LibraryImport(LibraryName)]
        public static partial int phys_run(IntPtr phys, double step_time, uint steps);

        [LibraryImport(LibraryName)]
        public static partial double phys_get_density(IntPtr phys);

        [LibraryImport(LibraryName)]
        public static partial void phys_set_density(IntPtr phys, double density);

        [LibraryImport(LibraryName)]
        public static partial IntPtr phys_ref_accel_of_gravity(IntPtr phys);

        [LibraryImport(LibraryName)]
        public static partial IntPtr phys_ref_wind(IntPtr phys);

        [LibraryImport(LibraryName)]
        public static partial IntPtr phys_ref_object(IntPtr phys, uint id);

        [LibraryImport(LibraryName)]
        public static partial uint phys_get_objects_num(IntPtr phys);

        [LibraryImport(LibraryName)]
        public static partial double phys_get_time(IntPtr phys);

        [LibraryImport(LibraryName)]
        public static partial void phys_destroy(IntPtr phys);

        [LibraryImport(LibraryName)]
        public static partial IntPtr pobj_ref_pos(IntPtr pobj);

        [LibraryImport(LibraryName)]
        public static partial IntPtr pobj_ref_mov(IntPtr pobj);

        [LibraryImport(LibraryName)]
        public static partial double pobj_get_mass(IntPtr pobj);

        [LibraryImport(LibraryName)]
        public static partial void pobj_set_mass(IntPtr pobj, double mass);

        [LibraryImport(LibraryName)]
        public static partial double pobj_get_radius(IntPtr pobj);

        [LibraryImport(LibraryName)]
        public static partial void pobj_set_radius(IntPtr pobj, double radius);

        [LibraryImport(LibraryName)]
        public static partial void pvec_set_scs(IntPtr pvec, double len, double angle);

        [LibraryImport(LibraryName)]
        public static partial double pvec_get_x(IntPtr pvec);

        [LibraryImport(LibraryName)]
        public static partial double pvec_get_y(IntPtr pvec);

        [LibraryImport(LibraryName)]
        public static partial void pvec_set_x(IntPtr pvec, double x);

        [LibraryImport(LibraryName)]
        public static partial void pvec_set_y(IntPtr pvec, double y);

        [LibraryImport(LibraryName)]
        public static partial double pvec_get_len(IntPtr pvec);

        [LibraryImport(LibraryName)]
        public static partial double pvec_get_angle(IntPtr pvec);

        [LibraryImport(LibraryName)]
        public static partial IntPtr phys_strerror(int result);
    }


    public class Vector
    {
        private readonly IntPtr _nativePtr;

        internal Vector(IntPtr nativePtr)
        {
            _nativePtr = nativePtr;
        }

        public void SetPolar(double length, double angle) => LibFlPhys.pvec_set_scs(_nativePtr, length, angle);

        public double X
        {
            get => LibFlPhys.pvec_get_x(_nativePtr);
            set => LibFlPhys.pvec_set_x(_nativePtr, value);
        }

        public double Y
        {
            get => LibFlPhys.pvec_get_y(_nativePtr);
            set => LibFlPhys.pvec_set_y(_nativePtr, value);
        }

        public double Length => LibFlPhys.pvec_get_len(_nativePtr);
        public double Angle => LibFlPhys.pvec_get_angle(_nativePtr);
    }

    public class PhysicalObject
    {
        private readonly IntPtr _nativePtr;

        internal PhysicalObject(IntPtr nativePtr)
        {
            _nativePtr = nativePtr;
        }

        public Vector Position => new Vector(LibFlPhys.pobj_ref_pos(_nativePtr));
        public Vector Movement => new Vector(LibFlPhys.pobj_ref_mov(_nativePtr));

        public double Mass
        {
            get => LibFlPhys.pobj_get_mass(_nativePtr);
            set => LibFlPhys.pobj_set_mass(_nativePtr, value);
        }

        public double Radius
        {
            get => LibFlPhys.pobj_get_radius(_nativePtr);
            set => LibFlPhys.pobj_set_radius(_nativePtr, value);
        }

    }



    public static class PhysError
    {
        public static string GetString(int result)
        {
            var ptr = LibFlPhys.phys_strerror(result);
            return Marshal.PtrToStringUTF8(ptr) ?? $"Unknown error code: {result}";
        }
    }


    public class PhysicsSystem : IDisposable
    {
        private IntPtr _nativePtr;

        public PhysicsSystem(uint objects_num)
        {
            _nativePtr = LibFlPhys.phys_create(objects_num);
            if (_nativePtr == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create physics system");
        }

        public double Density
        {
            get => LibFlPhys.phys_get_density(_nativePtr);
            set => LibFlPhys.phys_set_density(_nativePtr, value);
        }

        public Vector AccelerationOfGravity => new Vector(LibFlPhys.phys_ref_accel_of_gravity(_nativePtr));
        public Vector Wind => new Vector(LibFlPhys.phys_ref_wind(_nativePtr));

        public PhysicalObject GetObject(uint id)
        {
            var objPtr = LibFlPhys.phys_ref_object(_nativePtr, id);
            if (objPtr == IntPtr.Zero)
                throw new ArgumentException($"Object with id {id} not exist");

            return new PhysicalObject(objPtr);
        }

        public uint ObjectsCount => LibFlPhys.phys_get_objects_num(_nativePtr);

        public bool IsGravityEnabled
        {
            get => LibFlPhys.phys_get_is_gravity(_nativePtr) == 0;
            set => LibFlPhys.phys_set_is_gravity(_nativePtr, value ? 1 : 0);
        }

        public double Time => LibFlPhys.phys_get_time(_nativePtr);

        public int Run(double step_time, uint steps)
        {
            return LibFlPhys.phys_run(_nativePtr, step_time, steps);
        }

        public void Dispose()
        {
            if (_nativePtr != IntPtr.Zero)
            {
                LibFlPhys.phys_destroy(_nativePtr);
                _nativePtr = IntPtr.Zero;
            }
        }
    }



}