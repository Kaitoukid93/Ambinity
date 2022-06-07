
using adrilight.View;
using adrilight.ViewModel;
using Microsoft.Win32;
using Ninject;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using Ninject.Extensions.Conventions;
using adrilight.Resources;
using adrilight.Util;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using adrilight.Ninject;
using adrilight.Spots;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using System.Threading;
using Un4seen.BassWasapi;
using Un4seen.Bass;
using System.Windows.Media;
using HandyControl.Themes;

namespace adrilight
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
     //structures to display bitmap image after getting data from shader
    public struct Pixel
    {
        public byte R, G, B;
        public Pixel(byte r, byte g, byte b)
        {
            R = r; G = g; B = b;
        }

        public Int32 GetBPP24RGB_Int32()
        {
            return R << 16 | G << 8 | B;
        }
        public Int32 GetBPP16RGB_Int32()
        {
            return ((R & 0xF8) << 16) | ((G & 0xFC) << 8) | (B & 0xF8);
        }
        public Int32 GetBPP8RGB_Int32()
        {
            return ((R & 0xE0) << 16) | ((G & 0xE0) << 8) | (B & 0xC0);
        }
        public Int32 GetBPP8Grayscale_Int32()
        {
            byte color = (byte)((R + G + B) / 3);
            return color << 16 | color << 8 | color;
        }
        public Int32 GetBPP1Monochrome_Int32()
        {
            byte color = (byte)(((R + G + B) / 3) > 127 ? 255 : 0);
            return color << 16 | color << 8 | color;
        }
    }


    public sealed partial class App : System.Windows.Application
    {
        private static readonly ILogger _log = LogManager.GetCurrentClassLogger();
        private static System.Threading.Mutex _mutex = null;
        private static Mutex _adrilightMutex;
        protected override void OnStartup(StartupEventArgs startupEvent)
        {




            _adrilightMutex = new Mutex(true, "adrilight2");
            if (!_adrilightMutex.WaitOne(TimeSpan.Zero, true))
            {
                //another instance is already running!
                HandyControl.Controls.MessageBox.Show("Adrilight đã được khởi chạy trước đó rồi, vui lòng kiểm tra Task Manager hoặc System Tray Icon"
                    , "App đã được khởi chạy!");
                Shutdown();
                return;
            }

            SetupDebugLogging();
            SetupLoggingForProcessWideEvents();

            base.OnStartup(startupEvent);

            _log.Debug($"adrilight {VersionNumber}: Main() started.");
            kernel = SetupDependencyInjection(false);



            this.Resources["Locator"] = new ViewModelLocator(kernel);


            // DeviceSettings = kernel.Get<IDeviceSettings>();
            // UserSettings = kernel.Get<IUserSettings>();
            GeneralSettings = kernel.Get<IGeneralSettings>();
            _telemetryClient = kernel.Get<TelemetryClient>();

            //SetupNotifyIcon();




            if(GeneralSettings.ThemeIndex==1)
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            }
            else
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            }
            

            ThemeManager.Current.AccentColor = GeneralSettings.AccentColor;
            // Current.MainWindow = kernel.Get<MainView>();
            if (!GeneralSettings.StartMinimized)
            {
                OpenSettingsWindow();
            }
            
            SetupTrackingForProcessWideEvents(_telemetryClient);
        }

        protected override void OnExit(ExitEventArgs e)
        {


            base.OnExit(e);
            _adrilightMutex?.Dispose();

            LogManager.Shutdown();
        }
        protected void CloseMutexHandler(object sender, EventArgs startupEvent)
        {
            _mutex?.Close();
        }
        private TelemetryClient _telemetryClient;

        private static TelemetryClient SetupApplicationInsights(IDeviceSettings settings)
        {
            const string ik = "65086b50-8c52-4b13-9b05-92fbe69c7a52";
            TelemetryConfiguration.Active.InstrumentationKey = ik;
            var tc = new TelemetryClient {
                InstrumentationKey = ik
            };

            tc.Context.User.Id = "1234";//settings.InstallationId.ToString();
            tc.Context.Session.Id = Guid.NewGuid().ToString();
            tc.Context.Device.OperatingSystem = Environment.OSVersion.ToString();

            GlobalDiagnosticsContext.Set("user_id", tc.Context.User.Id);
            GlobalDiagnosticsContext.Set("session_id", tc.Context.Session.Id);
            return tc;
        }

       

        internal static IKernel SetupDependencyInjection(bool isInDesignMode)
        {

            var kernel = new StandardKernel(new DeviceSettingsInjectModule());
            kernel.Bind<MainViewViewModel>().ToSelf().InSingletonScope();
            kernel.Bind<MainView>().ToSelf().InSingletonScope();
            //Load setting từ file Json//
            var settingsManager = new UserSettingsManager();
            var existedDevice = settingsManager.LoadDeviceIfExists();

            kernel.Bind(x => x.FromThisAssembly()
              .SelectAllClasses()
              .BindAllInterfaces());
            //var desktopDuplicationReader = kernel.Get<IDesktopDuplicatorReader>();
            //var desktopDuplicationReaderSecondary = kernel.Get<IDesktopDuplicatorReaderSecondary>();
            //var desktopDuplicationReaderThird = kernel.Get<IDesktopDuplicatorReaderThird>();
            // var openRGBClient = kernel.Get<IOpenRGBClientDevice>();
            var serialDeviceDetection = kernel.Get<ISerialDeviceDetection>();
            //var shaderEffect = kernel.Get<IShaderEffect>();
            var context = kernel.Get<IContext>();
            var desktopFrame = kernel.GetAll<IDesktopFrame>();
            
            
            //var hotKeyManager = kernel.Get<IHotKeyManager>();
            kernel.Bind<IOpenRGBStream>().To<OpenRGBStream>().InSingletonScope();
            var openRGBStream = kernel.Get<IOpenRGBStream>();
            var rainbowTicker = kernel.Get<IRainbowTicker>();

            var audioFrame = kernel.Get<IAudioFrame>();
            //// tách riêng từng setting của từng device///
            if (existedDevice != null)
            {
                foreach (var device in existedDevice)
                {

                    
                   


                    var iD = device.DeviceID.ToString();
                    var outputs = device.AvailableOutputs.ToList();
                    if (device.UnionOutput != null)
                        outputs.Add(device.UnionOutput);
                    var connectionType = device.DeviceConnectionType;
                    if(connectionType!="OpenRGB")
                    {
                        switch (connectionType)
                        {
                            case "wired":
                                kernel.Bind<ISerialStream>().To<SerialStream>().InSingletonScope().Named(iD).WithConstructorArgument("deviceSettings", kernel.Get<IDeviceSettings>(iD));
                           
                                break;
                            case "wireless":
                                kernel.Bind<ISerialStream>().To<NetworkStream>().InSingletonScope().Named(iD).WithConstructorArgument("deviceSettings", kernel.Get<IDeviceSettings>(iD));

                                break;

                        }
                        var serialStream = kernel.Get<ISerialStream>(iD);
                    }
                   

                    foreach (var output in outputs)
                    {
                        
                        var outputID = iD + output.OutputID.ToString();
                        //kernel.Bind<IStaticColor>().To<StaticColor>().InSingletonScope().Named(DeviceName).WithConstructorArgument("deviceSettings", kernel.Get<IDeviceSettings>(DeviceName)).WithConstructorArgument("deviceSpotSet", kernel.Get<IDeviceSpotSet>(DeviceName));
                        kernel.Bind<IRainbow>().To<Rainbow>().InSingletonScope().Named(outputID).WithConstructorArgument("outputSettings", kernel.Get<IOutputSettings>(outputID));
                        kernel.Bind<IDeviceSpotSet>().To<DeviceSpotSet>().InSingletonScope().Named(outputID).WithConstructorArgument("outputSettings", kernel.Get<IOutputSettings>(outputID));
                        kernel.Bind<IMusic>().To<Music>().InSingletonScope().Named(outputID).WithConstructorArgument("outputSettings", kernel.Get<IOutputSettings>(outputID));
                        kernel.Bind<IDesktopDuplicatorReader>().To<DesktopDuplicatorReader>().InSingletonScope().Named(outputID).WithConstructorArgument("outputSettings", kernel.Get<IOutputSettings>(outputID));
                        kernel.Bind<IGifxelation>().To<Gifxelation>().InSingletonScope().Named(outputID).WithConstructorArgument("outputSettings", kernel.Get<IOutputSettings>(outputID));
                        kernel.Bind<IStaticColor>().To<StaticColor>().InSingletonScope().Named(outputID).WithConstructorArgument("outputSettings", kernel.Get<IOutputSettings>(outputID));
                        var spotset = kernel.Get<IDeviceSpotSet>(outputID);
                        var rainbow = kernel.Get<IRainbow>(outputID);
                        var screencapture = kernel.Get<IDesktopDuplicatorReader>(outputID);
                        var music = kernel.Get<IMusic>(outputID);
                        var staticColor = kernel.Get<IStaticColor>(outputID);
                        var gifxelation = kernel.Get<IGifxelation>(outputID);

                    }

                    

                }




            }

            return kernel;


        }

        private void SetupLoggingForProcessWideEvents()
        {
            AppDomain.CurrentDomain.UnhandledException +=
    (sender, args) => ApplicationWideException(sender, args.ExceptionObject as Exception, "CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (sender, args) => ApplicationWideException(sender, args.Exception, "DispatcherUnhandledException");

            // var desktopduplicators = kernel.GetAll<IDesktopDuplicatorReader>();
            Exit += (s, e) =>
            {
                var SerialStream = kernel.GetAll<ISerialStream>();
                foreach (var serialStream in SerialStream)
                {
                    serialStream.Stop();
                }

                _log.Debug("Application exit!");
            };



            SystemEvents.PowerModeChanged += (s, e) =>
            {
                _log.Debug("Changing Powermode to {0}", e.Mode);
                if (e.Mode == PowerModes.Resume)
                {
                    GC.Collect();
                    var SerialStream = kernel.GetAll<ISerialStream>();
                    foreach (var serialStream in SerialStream)
                    {
                        serialStream.Start();
                    }


                    //var desktopFrame = kernel.Get<IDesktopFrame>();
                    //var secondDesktopFrame = kernel.Get<ISecondDesktopFrame>();
                    //var thirdDesktopFrame = kernel.Get<IThirdDesktopFrame>();
                    //desktopFrame.RefreshCapturingState();
                    //secondDesktopFrame.RefreshCapturingState();
                    //thirdDesktopFrame.RefreshCapturingState();



                    _log.Debug("Restart the serial stream after sleep!");
                }
                else if (e.Mode == PowerModes.Suspend)
                {
                    var SerialStream = kernel.GetAll<ISerialStream>();
                    foreach (var serialStream in SerialStream)
                    {
                        serialStream.Stop();
                    }


                    //var desktopFrame = kernel.Get<IDesktopFrame>();
                    //var secondDesktopFrame = kernel.Get<ISecondDesktopFrame>();
                    //var thirdDesktopFrame = kernel.Get<IThirdDesktopFrame>();
                    //desktopFrame.Stop();
                    //secondDesktopFrame.Stop();
                    //thirdDesktopFrame.Stop();
                    _log.Debug("Stop the serial stream due to sleep condition!");
                }

            };
            SystemEvents.SessionEnding += (s, e) =>
            {
                var SerialStream = kernel.GetAll<ISerialStream>();
                foreach (var serialStream in SerialStream)
                {
                    serialStream.Stop();
                }
                _log.Debug("Stop the serial stream due to power down or log off condition!");
            };
        }



        private void SetupTrackingForProcessWideEvents(TelemetryClient tc)
        {
            if (tc == null)
            {
                throw new ArgumentNullException(nameof(tc));
            }

            AppDomain.CurrentDomain.UnhandledException += (sender, args) => tc.TrackException(args.ExceptionObject as Exception);

            DispatcherUnhandledException += (sender, args) => tc.TrackException(args.Exception);

            Exit += (s, e) => { tc.TrackEvent("AppExit"); tc.Flush(); };

            SystemEvents.PowerModeChanged += (s, e) => tc.TrackEvent("PowerModeChanged", new Dictionary<string, string> { { "Mode", e.Mode.ToString() } });
            tc.TrackEvent("AppStart");
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void SetupDebugLogging()
        {
            var config = new LoggingConfiguration();
            var debuggerTarget = new DebuggerTarget() { Layout = "${processtime} ${message:exceptionSeparator=\n\t:withException=true}" };
            config.AddTarget("debugger", debuggerTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, debuggerTarget));

            LogManager.Configuration = config;

            _log.Info($"DEBUG logging set up!");
        }


        private IKernel kernel;
        
        private void OpenSettingsWindow()
        {
            var _mainForm = kernel.Get<MainView>();
           
                //bring to front?
                _mainForm.Show();
            
        }
        //private void MainForm_FormClosed(object sender, EventArgs e)
        //{
        //    if (_mainForm == null) return;

        //    //deregister to avoid memory leak
        //    _mainForm.Closed -= MainForm_FormClosed;
        //    _mainForm = null;
        //}
        private void SetupNotifyIcon()

        {

            var icon = new System.Drawing.Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("adrilight.zoe.ico"));

            var contextMenu = new ContextMenuStrip();


            //var allDevices = kernel.GetAll<IDeviceSettings>();
            //foreach (var device in allDevices)
            //{
            //    if (!device.IsHUB && device.ParrentLocation == 151293)
            //    {
            //        var deviceMenu = new ToolStripMenuItem(device.DeviceName);

            //        deviceMenu.DropDownItems.Add(new ToolStripMenuItem("Bật/Tắt LED",null, (s, e) =>
            //        {
            //            if (device.LEDOn)
            //                device.LEDOn = false;
            //            else
            //                device.LEDOn = true;

            //        }));
            //        contextMenu.Items.Add(deviceMenu);
            //    }


            //}
            //var SyncAll = new ToolStripMenuItem("Đồng bộ tất cả", null, (s, e) =>
            //{


            //});
            //SyncAll.DropDownItems.Add(new ToolStripMenuItem("Theo màn hình", null, (s, e) =>
            //{
            //    foreach (var device in allDevices)
            //    {
            //        device.SelectedEffect = 0;
            //    }

            //}));

            //SyncAll.DropDownItems.Add(new ToolStripMenuItem("Màu tĩnh",null, (s, e) =>
            //        {
            //            foreach (var device in allDevices)
            //            {
            //                device.SelectedEffect = 2;
            //            }

            //        }));
            //SyncAll.DropDownItems.Add(new ToolStripMenuItem("Dải màu", null, (s, e) =>
            //{
            //    foreach (var device in allDevices)
            //    {
            //        device.SelectedEffect = 1;
            //    }

            //}));
            //SyncAll.DropDownItems.Add(new ToolStripMenuItem("Theo Nhạc", null, (s, e) =>
            //{
            //    foreach (var device in allDevices)
            //    {
            //        device.SelectedEffect = 3;
            //    }

            //}));
            //SyncAll.DropDownItems.Add(new ToolStripMenuItem("Atmosphere", null, (s, e) =>
            //{
            //    foreach (var device in allDevices)
            //    {
            //        device.SelectedEffect = 4;
            //    }

            //}));
            //SyncAll.DropDownItems.Add(new ToolStripMenuItem("Canvas Lighting", null, (s, e) =>
            //{
            //    foreach (var device in allDevices)
            //    {
            //        device.SelectedEffect = 5;
            //    }

            //}));
            var dashboard = new ToolStripMenuItem("Dashboard", null, (s, e) => OpenSettingsWindow());
            var exit = new ToolStripMenuItem("Thoát", null, (s, e) => Shutdown(0));
            //contextMenu.Items.Add(SyncAll);
            contextMenu.Items.Add(dashboard);
            contextMenu.Items.Add(exit);
            // contextMenu.Items.Add(new MenuItem("Dashboard", (s, e) => OpenNewUI()));
            //  contextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("Cài đặt...", (s, e) => OpenSettingsWindow()));
            //contextMenu.Items.Add(new MenuItem("Thoát", (s, e) => Shutdown(0)));

            //This Commented due to Net5 incompatible

            var notifyIcon = new System.Windows.Forms.NotifyIcon() {
                Text = $"adrilight {VersionNumber}",
                Icon = icon,
                Visible = true,
                ContextMenuStrip = contextMenu
            };
            //  notifyIcon.DoubleClick += (s, e) => { OpenSettingsWindow(); };
            notifyIcon.DoubleClick += (s, e) => { OpenSettingsWindow(); };
            notifyIcon.BalloonTipText = "Ứng dụng đã ẩn, double click để hiển thị cửa sổ";

            Exit += (s, e) => notifyIcon.Dispose();
        }


        public static string VersionNumber { get; } = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

        private IGeneralSettings GeneralSettings { get; set; }


        private void ApplicationWideException(object sender, Exception ex, string eventSource)
        {
            _log.Fatal(ex, $"ApplicationWideException from sender={sender}, adrilight version={VersionNumber}, eventSource={eventSource}");

            var sb = new StringBuilder();
            sb.AppendLine($"Sender: {sender}");
            sb.AppendLine($"Source: {eventSource}");
            if (sender != null)
            {
                sb.AppendLine($"Sender Type: {sender.GetType().FullName}");
            }
            sb.AppendLine("-------");
            do
            {
                sb.AppendLine($"exception type: {ex.GetType().FullName}");
                sb.AppendLine($"exception message: {ex.Message}");
                sb.AppendLine($"exception stacktrace: {ex.StackTrace}");
                sb.AppendLine("-------");
                ex = ex.InnerException;
            } while (ex != null);

            HandyControl.Controls.MessageBox.Show(sb.ToString(), "unhandled exception :-(");
            try
            {
                Shutdown(-1);
            }
            catch
            {
                Environment.Exit(-1);
            }
        }
    }

}
