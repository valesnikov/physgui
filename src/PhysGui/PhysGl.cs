using System.Runtime.InteropServices;

namespace PhysGui
{

    public static partial class LibFlPhysGl
    {
        private const string LibraryName = "flphys";

        [LibraryImport(LibraryName)]
        public static partial IntPtr physgl_create(IntPtr phys);

        [LibraryImport(LibraryName)]
        public static partial void physgl_set_phys(IntPtr phgl, IntPtr phys);

        [LibraryImport(LibraryName)]
        public static partial void physgl_update(IntPtr phgl);

        [LibraryImport(LibraryName)]
        public static partial void physgl_on_resize(IntPtr phgl, double aspect_ratio);

        [LibraryImport(LibraryName)]
        public static partial void physgl_render(IntPtr phgl, double center_x, double center_y, double scale, double aspect);

        [LibraryImport(LibraryName)]
        public static partial void physgl_destroy(IntPtr phgl);
    }

    public sealed class PhysGl : IDrawable 
    {
        private IntPtr _handle = IntPtr.Zero;
        private PhysicsSystem phys;

        public PhysGl(PhysicsSystem phys)
        {
            this.phys = phys;
        }

        public void Realized()
        {
            _handle = LibFlPhysGl.physgl_create(phys.Ptr());
            if (_handle == IntPtr.Zero)
                throw new InvalidOperationException("Failed to initialize PhysGl.");
        }

        public void Update()
        {
            LibFlPhysGl.physgl_update(_handle);
        }

        public void Resize(double aspectRatio)
        {
            LibFlPhysGl.physgl_on_resize(_handle, aspectRatio);
        }

        public void Render(double centerX, double centerY, double scale, double aspect)
        {
            LibFlPhysGl.physgl_render(_handle, centerX, centerY, scale, aspect);
        }

        public void Unrealized()
        {
            LibFlPhysGl.physgl_destroy(_handle);
            _handle = IntPtr.Zero;
        }
    }
}
