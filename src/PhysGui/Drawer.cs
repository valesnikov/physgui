using Gtk;

namespace PhysGui
{
    public class Drawer
    {
        private PhysGl? gl = null;
        private int width = 800;
        private int height = 600;
        private double scale = 1;
        private double centerX = 0;
        private double centerY = 0;
        private double lastX = 0;
        private double lastY = 0;

        public Drawer() { }

        public void OnGLRealized(object? sender, EventArgs e)
        {
            var area = (GLArea)sender!;
            area.MakeCurrent();
            gl = new PhysGl(128);
        }

        public void OnGLRender(object? sender, EventArgs e)
        {
            var area = (GLArea)sender!;
            area.MakeCurrent();
            gl?.PreviewRender(centerX, centerY, scale);
        }

        public void OnGLResize(object? sender, EventArgs e)
        {
            var area = (GLArea)sender!;
            if ((width = area.AllocatedWidth) < 1)
            {
                width = 1;
            }
            if ((height = area.AllocatedHeight) < 1)
            {
                height = 1;
            }
            area.MakeCurrent();
            gl?.OnResize(width, height);
        }

        public void OnMouseDown(object? sender, ButtonPressEventArgs args)
        {
            lastX = args.Event.X;
            lastY = args.Event.Y;
        }

        public void OnMouseUp(object? sender, ButtonReleaseEventArgs args)
        {
            lastX = args.Event.X;
            lastY = args.Event.Y;
        }

        public void OnMouseMove(object? sender, MotionNotifyEventArgs args)
        {
            var area = (GLArea)sender!;

            var x = args.Event.X;
            var y = args.Event.Y;

            var state = args.Event.State;
            bool leftButtonPressed = (state & Gdk.ModifierType.Button1Mask) != 0;
            bool middleButtonPressed = (state & Gdk.ModifierType.Button2Mask) != 0;
            bool rightButtonPressed = (state & Gdk.ModifierType.Button3Mask) != 0;

            if (leftButtonPressed)
            {

                double deltaX = (x - lastX) / width * 2 * width / height / scale;
                double deltaY = (lastY - y) / height * 2 / scale;
                centerX -= deltaX;
                centerY -= deltaY;
                lastX = x;
                lastY = y;
                area.QueueRender();
            }
            else
            {
                lastX = x;
                lastY = y;
            }
        }

        public void OnMouseScroll(object? sender, ScrollEventArgs args)
        {
            var area = (GLArea)sender!;

            double oldScale = scale;
            scale *= (args.Event.Direction == Gdk.ScrollDirection.Up) ? 1.1 : 1.0 / 1.1;
            double k = 1.0 / oldScale - 1.0 / scale;
            double aspect = (double)width / height;

            centerX += (args.Event.X / width * 2.0 - 1.0) * aspect * k;
            centerY += (1.0 - args.Event.Y / height * 2.0) * k;

            area.QueueRender();
        }
    }
}