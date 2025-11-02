using System.Diagnostics;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace PhysGui
{
    class MainWindow : Window
    {
        [UI] private Button? startButton = null;
        [UI] private Button? stopButton = null;
        [UI] private GLArea? glArea = null;

        private Scene scene;

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

            scene = new Scene(glArea!);
            var cfg = new PhysicsConfigParser("test.yaml");
            var phys = cfg.createPhysicsSystem();
            var physgl = new PhysGl(phys);
            scene.AddDrawable(physgl);

            scene.OnRealized += () =>
            {
                var t = new Thread(() => GameLoop(phys, physgl))
                {
                    IsBackground = true
                };
                t.Start();
            };
        }

        private void GameLoop(PhysicsSystem phys, PhysGl physgl)
        {
            const long targetFPS = 100;
            const double targetFrameTime = 1000.0 / targetFPS;
            long accCoef = 10000;

            while (true)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                //----------------------------------------------
                phys.Run((1.0 / targetFPS) / accCoef, accCoef);
                physgl.Update();

                //----------------------------------------------
                stopwatch.Stop();
                double elapsed = stopwatch.Elapsed.TotalMilliseconds;
                double remainingTime = targetFrameTime - elapsed;
                if (remainingTime > 0)
                    Thread.Sleep((int)remainingTime);
            }

        }



        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }
    }
}
