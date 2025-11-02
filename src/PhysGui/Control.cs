
using Gdk;

namespace PhysGui
{
    public class Control
    {
        private int width = 800;
        private int height = 600;
        private double scale = 1;
        private double aspect = 800.0 / 600.0;

        private (double x, double y) center = (0, 0);
        private (double x, double y) last = (0, 0);

        public Control() { }

        public Control((double x, double y) center, double scale)
        {
            this.center = center;
            this.scale = scale;
        }

        public double Aspect => aspect;
        public double Scale => scale;
        public (double x, double y) Center => center;

        public void OnResize(int width, int height)
        {
            this.width = width;
            this.height = height;
            this.aspect = (double)width / height;
        }

        public void OnMouse(double x, double y, ModifierType buttons)
        {
            bool leftButton = (buttons & Gdk.ModifierType.Button1Mask) != 0;
            bool middleButton = (buttons & Gdk.ModifierType.Button2Mask) != 0;
            bool rightButton = (buttons & Gdk.ModifierType.Button3Mask) != 0;
            if (leftButton)
            {
                double deltaX = (x - last.x) / width * 2 * width / height / scale;
                double deltaY = (last.y - y) / height * 2 / scale;
                center.x -= deltaX;
                center.y -= deltaY;
            }
            last.x = x;
            last.y = y;
        }

        public void OnScroll(double offset) // -1 and 1 for mouse wheel
        {
            var oldScale = scale;
            scale *= Math.Pow(1.1, offset);
            double k = 1.0 / oldScale - 1.0 / scale;
            double aspect = (double)width / height;
            center.x += (last.x / width * 2.0 - 1.0) * aspect * k;
            center.y += (1.0 - last.y / height * 2.0) * k;
        }

    }

}