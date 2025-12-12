using System.Collections.Concurrent;
using System.Drawing;
using System.Runtime.InteropServices;

namespace PhysGui
{

    public static partial class BackGLNative
    {
        private const string LibraryName = "flphys";

        [LibraryImport(LibraryName)]
        public static partial IntPtr backgl_builder_create();

        [LibraryImport(LibraryName)]
        public static partial void backgl_builder_add(
            IntPtr bglb,
            float x1, float y1,
            float x2, float y2,
            float x3, float y3,
            byte r, byte g, byte b
        );

        [LibraryImport(LibraryName)]
        public static partial void backgl_builder_set_background_color(
            IntPtr bglb,
            byte r,
            byte g,
            byte b
        );

        [LibraryImport(LibraryName)]
        public static partial IntPtr backgl_builder_build(IntPtr bglb);

        [LibraryImport(LibraryName)]
        public static partial void backgl_builder_cancel(IntPtr bglb);

        [LibraryImport(LibraryName)]
        public static partial void backgl_render(
            IntPtr bgl,
            double center_x,
            double center_y,
            double scale,
            double aspect
        );

        [LibraryImport(LibraryName)]
        public static partial void backgl_destroy(IntPtr bgl);

    }

    public readonly struct Triangle
    {
        public readonly (float x, float y) V1;
        public readonly (float x, float y) V2;
        public readonly (float x, float y) V3;
        public readonly Color Color;

        public Triangle(
            (float x, float y) v1,
            (float x, float y) v2,
            (float x, float y) v3,
            Color color)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
            Color = color;
        }
    }

    public sealed class BackGlBuilder
    {
        private IntPtr handle;

        public BackGlBuilder()
        {
            handle = BackGLNative.backgl_builder_create();
            if (handle == IntPtr.Zero)
                throw new Exception("Failed to create backgl_builder.");
        }

        public BackGlBuilder Add(Triangle tri)
        {
            if (handle == IntPtr.Zero)
                throw new ObjectDisposedException(nameof(BackGlBuilder));

            BackGLNative.backgl_builder_add(
                handle,
                tri.V1.x, tri.V1.y,
                tri.V2.x, tri.V2.y,
                tri.V3.x, tri.V3.y,
                tri.Color.R, tri.Color.G, tri.Color.B
            );

            return this;
        }

        public BackGlBuilder SetBgColor(Color c)
        {
            if (handle == IntPtr.Zero)
                throw new ObjectDisposedException(nameof(BackGlBuilder));
            BackGLNative.backgl_builder_set_background_color(handle, c.R, c.G, c.B);
            return this;
        }

        public void Cancel()
        {
            BackGLNative.backgl_builder_cancel(handle);
            handle = IntPtr.Zero;
        }

        public BackGL Build()
        {
            var res = BackGLNative.backgl_builder_build(handle);
            if (res == IntPtr.Zero)
                throw new Exception("backgl_builder_build returned null pointer.");
            handle = IntPtr.Zero;
            return new BackGL(res);
        }
    }

    public sealed class BackGL : IDisposable
    {
        private IntPtr handle;

        public BackGL(IntPtr handle)
        {
            this.handle = handle;
        }

        public void Render(double centerX, double centerY, double scale, double aspect)
        {
            if (handle == IntPtr.Zero)
            {
                throw new ObjectDisposedException("BackGL");
            }
            BackGLNative.backgl_render(handle, centerX, centerY, scale, aspect);
        }

        public void Dispose()
        {
            if (handle != IntPtr.Zero)
            {
                BackGLNative.backgl_destroy(handle);
                handle = IntPtr.Zero;
            }
        }
    }

    public class BackGlWrapper : IDrawable
    {
        private BackGL? backGl = null;

        private readonly ConcurrentQueue<BackGlBuilder> queue = new ConcurrentQueue<BackGlBuilder>();

        public void set(BackGlBuilder backGlBuilder)
        {
            queue.Enqueue(backGlBuilder);
        }

        public void Realized()
        {
            BackGlBuilder? selected = null;
            while (queue.TryDequeue(out var builder))
            {
                selected?.Cancel();
                selected = builder;
            }
            if (selected != null)
            {
                backGl?.Dispose();
                backGl = selected.Build();
            }
        }

        public void Render(double centerX, double centerY, double scale, double aspect)
        {
            Realized();
            backGl?.Render(centerX, centerY, scale, aspect);
        }

        public void Unrealized()
        {
            while (queue.TryDequeue(out var builder))
            {
                builder.Cancel();
            }
            backGl?.Dispose();
            backGl = null;
        }
    }
}