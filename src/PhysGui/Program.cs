using Gtk;

// yaml parser
// visualization
// start/stop
// step

namespace PhysGui
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {

            var a = new PhysicsConfigParser();
            a.ParseFile("test.yaml");
            using var phys = a.createPhysicsSystem();

            Application.Init();

            var app = new Application("org.physgui.physgui", GLib.ApplicationFlags.None);
            app.Register(GLib.Cancellable.Current);

            var win = new MainWindow();
            app.AddWindow(win);

            win.Show();
            Application.Run();
        }
    }
}
