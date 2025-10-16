using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace PhysGui
{
    class MainWindow : Window
    {
        [UI] private Button? startButton = null;
        [UI] private Button? stopButton = null;
        [UI] private GLArea? glArea = null;

        private Drawer drawer = new Drawer();
        private int _counter;

        public MainWindow() : this(new Builder("MainWindow.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
            if (startButton == null)
                throw new InvalidOperationException("startButton not found in Glade file");
            if (stopButton == null)
                throw new InvalidOperationException("stopButton not found in Glade file");
            if (glArea == null)
                throw new InvalidOperationException("glArea not found in Glade file");
            DeleteEvent += Window_DeleteEvent;


            glArea.Events |= Gdk.EventMask.ButtonPressMask
               | Gdk.EventMask.ButtonReleaseMask
               | Gdk.EventMask.PointerMotionMask
               | Gdk.EventMask.ScrollMask;

            glArea.SetRequiredVersion(3, 3);

            glArea.Realized += drawer.OnGLRealized;
            glArea.Render += drawer.OnGLRender;
            glArea.Resize += drawer.OnGLResize;
            glArea.ButtonPressEvent += drawer.OnMouseDown;
            glArea.ButtonReleaseEvent += drawer.OnMouseUp;
            glArea.MotionNotifyEvent += drawer.OnMouseMove;
            glArea.ScrollEvent += drawer.OnMouseScroll;
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }

        private void Button1_Clicked(object sender, EventArgs a)
        {
            _counter++;
        }
    }
}
