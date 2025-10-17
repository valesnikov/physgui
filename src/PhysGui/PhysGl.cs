using System.Runtime.InteropServices;

namespace PhysGui
{

    public static partial class LibFlPhysGl
    {
        private const string LibraryName = "flphys";

        [LibraryImport(LibraryName)]
        public static partial IntPtr physgl_init();

        [LibraryImport(LibraryName)]
        public static partial void physgl_on_resize(IntPtr phgl, int width, int height);

        [LibraryImport(LibraryName)]
        public static partial void physgl_preview_render(IntPtr phgl, double center_x, double center_y, double scale);

        [LibraryImport(LibraryName)]
        public static partial void physgl_destroy(IntPtr phgl);
    }

    public sealed class PhysGl : IDisposable
    {
        private IntPtr _handle;

        public PhysGl()
        {

            _handle = LibFlPhysGl.physgl_init();
            if (_handle == IntPtr.Zero)
                throw new InvalidOperationException("Failed to initialize PhysGl.");
        }

        public void OnResize(int width, int height)
        {
            LibFlPhysGl.physgl_on_resize(_handle, width, height);
        }

        public void PreviewRender(double centerX, double centerY, double scale)
        {
            LibFlPhysGl.physgl_preview_render(_handle, centerX, centerY, scale);
        }

        public void Dispose()
        {
            LibFlPhysGl.physgl_destroy(_handle);
            _handle = IntPtr.Zero;
        }

    }
}
