using Gtk;
using Action = System.Action;

namespace PhysGui
{
    public class Scene
    {
        private readonly Control control = new Control();
        private readonly GLArea area;

        private readonly List<IDrawable> drawable = new();

        public event Action? OnRealized;

        public Scene(GLArea glArea)
        {
            area = glArea;

            glArea.Events |= Gdk.EventMask.ButtonPressMask
               | Gdk.EventMask.ButtonReleaseMask
               | Gdk.EventMask.PointerMotionMask
               | Gdk.EventMask.ScrollMask;

            glArea.SetRequiredVersion(3, 3);
            glArea.Realized += OnGLRealized;
            glArea.Render += OnGLRender;
            glArea.Resize += OnGLResize;
            glArea.MotionNotifyEvent += OnMouse;
            glArea.ScrollEvent += OnMouseScroll;
        }

        public void AddDrawable(IDrawable obj)
        {
            if (obj == null) return;
            drawable.Add(obj);
        }

        public void RemoveDrawable(IDrawable obj)
        {
            if (obj == null) return;
            drawable.Remove(obj);
        }

        public void ClearDrawables()
        {
            drawable.Clear();
        }

        private void OnGLRealized(object? sender, EventArgs e)
        {
            area.AddTickCallback((_, _) =>
            {
                area.QueueRender();
                return true;
            });
            area.MakeCurrent();
            foreach (var obj in drawable)
                obj.Realized();

            OnRealized?.Invoke();
        }

        private void OnGLRender(object? sender, EventArgs e)
        {
            area.MakeCurrent();
            foreach (var obj in drawable)
                obj.Render(control.Center.x, control.Center.y, control.Scale, control.Aspect);
        }

        private void OnGLResize(object? sender, EventArgs e)
        {
            control.OnResize(area.AllocatedWidth, area.AllocatedHeight);
        }

        private void OnMouse(object? sender, MotionNotifyEventArgs args)
        {
            control.OnMouse(args.Event.X, args.Event.Y, args.Event.State);
        }

        private void OnMouseScroll(object? sender, ScrollEventArgs args)
        {
            control.OnScroll(args.Event.Direction == Gdk.ScrollDirection.Up ? 1 : -1);
        }
    }
}
