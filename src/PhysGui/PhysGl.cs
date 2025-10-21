using System.Runtime.InteropServices;

namespace PhysGui
{

    public static partial class LibFlPhysGl
    {
        private const string LibraryName = "flphys";

        [LibraryImport(LibraryName)]
        public static partial IntPtr physgl_create();

        [LibraryImport(LibraryName)]
        public static partial void physgl_on_resize(IntPtr phgl, double aspect_ratio);

        [LibraryImport(LibraryName)]
        public static partial void physgl_preview_render(IntPtr phgl, double center_x, double center_y, double scale);

        [LibraryImport(LibraryName)]
        public static partial void physgl_destroy(IntPtr phgl);
    }

    public sealed class PhysGl : IDrawable
    {
        private IntPtr _handle = IntPtr.Zero;

        public PhysGl()
        { }

        public void Realized()
        {
            _handle = LibFlPhysGl.physgl_create();
            if (_handle == IntPtr.Zero)
                throw new InvalidOperationException("Failed to initialize PhysGl.");
        }

        public void Resize(double aspectRatio)
        {
            LibFlPhysGl.physgl_on_resize(_handle, aspectRatio);
        }

        public void Render(double centerX, double centerY, double scale)
        {
            LibFlPhysGl.physgl_preview_render(_handle, centerX, centerY, scale);
        }

        public void Unrealized()
        {
            LibFlPhysGl.physgl_destroy(_handle);
            _handle = IntPtr.Zero;
        }
    }
}
