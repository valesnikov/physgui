using Gtk;

namespace PhysGui
{
    public class Drawer
    {
        private List<IDrawable> drawable = new List<IDrawable>();
        private int width = 800;
        private int height = 600;
        private double scale = 1;
        private (double x, double y) center = (0, 0);
        private (double x, double y) last = (0, 0);

        public Drawer()
        {
            drawable.Add(new PhysGl());
        }

        public void OnGLRealized(object? sender, EventArgs e)
        {
            var area = (GLArea)sender!;
            area.MakeCurrent();
            foreach (var obj in drawable)
                obj.Realized();
        }

        public void OnGLRender(object? sender, EventArgs e)
        {
            var area = (GLArea)sender!;
            area.MakeCurrent();
            foreach (var obj in drawable)
                obj.Render(center.x, center.y, scale);
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
            foreach (var obj in drawable)
                obj.Resize((double)width / height);
        }

        public void OnMouseDown(object? sender, ButtonPressEventArgs args)
        {
            last = (args.Event.X, args.Event.Y);
        }

        public void OnMouseUp(object? sender, ButtonReleaseEventArgs args)
        {
            last = (args.Event.X, args.Event.Y);
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
                double deltaX = (x - last.x) / width * 2 * width / height / scale;
                double deltaY = (last.y - y) / height * 2 / scale;
                center.x -= deltaX;
                center.y -= deltaY;
                last.x = x;
                last.y = y;
                area.QueueRender();
            }
            else
            {
                last.x = x;
                last.y = y;
            }
        }

        public void OnTouchpadScroll(object? sender, ScaleChangedArgs args)
        {
            Console.WriteLine($"Zoom scale: {args.Scale}");
        }

        public void OnMouseScroll(object? sender, ScrollEventArgs args)
        {
            var area = (GLArea)sender!;

            double oldScale = scale;
            scale *= (args.Event.Direction == Gdk.ScrollDirection.Up) ? 1.1 : 1.0 / 1.1;
            double k = 1.0 / oldScale - 1.0 / scale;
            double aspect = (double)width / height;

            center.x += (args.Event.X / width * 2.0 - 1.0) * aspect * k;
            center.y += (1.0 - args.Event.Y / height * 2.0) * k;

            area.QueueRender();
        }
    }
}