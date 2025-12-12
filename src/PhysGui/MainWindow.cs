using System.Diagnostics;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace PhysGui
{
    class MainWindow : Window
    {
        [UI] private Button startButton;
        [UI] private Button stopButton;
        [UI] private Button restartButton;
        [UI] private Scale speedScale;
        [UI] private Label speedLabel;
        [UI] private Button openButton;

        [UI] private GLArea glArea;

        private Scene scene;

        private double speed = 1;
        private volatile bool restartRequired = false;
        private ManualResetEventSlim started = new ManualResetEventSlim(false);
        private AutoResetEvent waitConfigSelect = new AutoResetEvent(false);

        private PhysicsConfigParser? config = null;
        private string? configPath = null;

        private BackGlWrapper back = new();

        public MainWindow() : this(new Builder("MainWindow.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
            if (startButton == null)
                throw new InvalidOperationException("startButton not found in Glade file");
            if (stopButton == null)
                throw new InvalidOperationException("stopButton not found in Glade file");
            if (restartButton == null)
                throw new InvalidOperationException("restartButton not found in Glade file");
            if (speedScale == null)
                throw new InvalidOperationException("speedScale not found in Glade file");
            if (speedLabel == null)
                throw new InvalidOperationException("speedLabel not found in Glade file");
            if (openButton == null)
                throw new InvalidOperationException("openButton not found in Glade file");
            if (glArea == null)
                throw new InvalidOperationException("glArea not found in Glade file");

            DeleteEvent += Window_DeleteEvent;

            scene = new Scene(glArea!);

            var physgl = new PhysGl();

            scene.AddDrawable(back);
            scene.AddDrawable(physgl);


            startButton.Clicked += (sender, args) =>
            {
                started.Set();
            };

            stopButton.Clicked += (sender, args) =>
            {
                started.Reset();
            };

            restartButton.Clicked += (sender, args) =>
            {
                speedScale.Value = 0.5;
                restartRequired = true;
                started.Set();
            };

            openButton.Clicked += FileChoose;

            speedScale.ValueChanged += (sender, args) =>
            {
                var min = 0.1;
                var max = 10;
                double value = min * Math.Pow(max / min, speedScale.Value);

                Interlocked.Exchange(ref speed, value);

                if (value >= 10)
                {
                    speedLabel.Text = value.ToString("F1");
                }
                else if (value >= 1)
                {
                    speedLabel.Text = value.ToString("F2");
                }
                else
                {
                    speedLabel.Text = value.ToString("F3").TrimStart('0');
                }
            };

            scene.OnRealized += () =>
            {
                var t = new Thread(() => GameLoop(physgl))
                {
                    IsBackground = true
                };
                t.Start();
            };
        }

        private void GameLoop(PhysGl physgl)
        {
            const long targetFPS = 100;
            const double targetFrameTime = 1000.0 / targetFPS;
            long accCoef = 10000;

            PhysicsSystem? phys = null;

            restartRequired = true;
            while (true)
            {
                if (restartRequired)
                {
                    while (config == null)
                    {
                        waitConfigSelect.WaitOne();
                    }
                    
                    restartRequired = false;
                    phys?.Dispose();
                    phys = config.createPhysicsSystem();
                    back.set(config.createBack());
                    scene.Control.Set(config.getCameraPosition());
                    started.Reset();
                }

                Stopwatch stopwatch = Stopwatch.StartNew();
                //----------------------------------------------

                double localSpeed = Interlocked.CompareExchange(ref speed, 0.0, 0.0);
                phys!.Run((1.0 / targetFPS) / accCoef * localSpeed, accCoef);
                physgl.Update(phys!);

                //----------------------------------------------
                stopwatch.Stop();
                double elapsed = stopwatch.Elapsed.TotalMilliseconds;
                double remainingTime = targetFrameTime - elapsed;
                if (remainingTime > 0)
                {
                    Thread.Sleep((int)remainingTime);
                }
                started.Wait();
            }

        }

        private void FileChoose(object? sender, EventArgs args)
        {
            FileChooserDialog dialog = new FileChooserDialog(
                "Select YAML file",
                this,
                FileChooserAction.Open,
                "Cancel", ResponseType.Cancel,
                "Open", ResponseType.Accept
            );

            FileFilter yamlFilter = new FileFilter();
            yamlFilter.Name = "YAML files";
            yamlFilter.AddPattern("*.yaml");
            yamlFilter.AddPattern("*.yml");

            dialog.Filter = yamlFilter;

            if (dialog.Run() == (int)ResponseType.Accept)
            {
                onFileOpen(dialog.Filename);
            }
            dialog.Destroy();
        }

        private void ShowErrorDialog(string message)
        {
            using (var dialog = new MessageDialog(
                this,
                DialogFlags.Modal,
                MessageType.Error,
                ButtonsType.Ok,
                message))
            {
                dialog.Run();
                dialog.Destroy();
            }
        }


        private void onFileOpen(string path)
        {
            var old = config;
            try
            {
                config = new PhysicsConfigParser(path);
                restartRequired = true;
                waitConfigSelect.Set();
                started.Set();
            }
            catch (FileNotFoundException ex)
            {
                ShowErrorDialog($"File not found:\n{ex.Message}");
                config = old;
            }
            catch (IOException ex)
            {
                ShowErrorDialog($"I/O error:\n{ex.Message}");
                config = old;
            }
            catch (Exception ex) when (ex is FormatException
                                       || ex is InvalidDataException
                                       || ex is YamlDotNet.Core.YamlException)
            {
                ShowErrorDialog($"Configuration format error:\n{ex.Message}");
                config = old;
            }
            catch (Exception ex)
            {
                ShowErrorDialog($"Unexpected error:\n{ex.Message}");
                config = old;
            }
        }


        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }
    }
}
