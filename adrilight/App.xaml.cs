
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

        protected override void OnStartup(StartupEventArgs startupEvent)
        {
            
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
                return;
            //string mutexId = ((System.Runtime.InteropServices.GuidAttribute)System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false).GetValue(0)).Value.ToString();
            //_mutex = new System.Threading.Mutex(true, mutexId, out bool createdNew);
            //if (!createdNew) Current.Shutdown();
            //else Exit += CloseMutexHandler;
            //  IServiceProvider serviceProvider = CreateServiceProvider();
            base.OnStartup(startupEvent);

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

            SetupNotifyIcon();

           

            Current.MainWindow = kernel.Get<MainView>();
            if (!GeneralSettings.StartMinimized)
            {
                Current.MainWindow.Show();
            }

            SetupTrackingForProcessWideEvents(_telemetryClient);
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
            var tc = new TelemetryClient
            {
                InstrumentationKey = ik
            };

            tc.Context.User.Id = settings.InstallationId.ToString();
            tc.Context.Session.Id = Guid.NewGuid().ToString();
            tc.Context.Device.OperatingSystem = Environment.OSVersion.ToString();

            GlobalDiagnosticsContext.Set("user_id", tc.Context.User.Id);
            GlobalDiagnosticsContext.Set("session_id", tc.Context.Session.Id);
            return tc;
        }

       

        internal static IKernel SetupDependencyInjection(bool isInDesignMode)
        {

            var kernel = new StandardKernel(new DeviceSettingsInjectModule());
           
            //Load setting từ file Json//
            var settingsManager = new UserSettingsManager();          
            var alldevicesettings = settingsManager.LoadDeviceIfExists();
        
            kernel.Bind(x => x.FromThisAssembly()
              .SelectAllClasses()
              .BindAllInterfaces());
            var desktopDuplicationReader = kernel.Get<IDesktopDuplicatorReader>();
           var desktopDuplicationReaderSecondary = kernel.Get<IDesktopDuplicatorReaderSecondary>();
            var desktopDuplicationReaderThird = kernel.Get<IDesktopDuplicatorReaderThird>();
            var openRGBClient = kernel.Get<IOpenRGBClientDevice>();
            var serialDeviceDetection = kernel.Get<ISerialDeviceDetection>();
            var shaderEffect = kernel.Get<IShaderEffect>();
            var context = kernel.Get<IContext>();
            //// tách riêng từng setting của từng device///
            if (alldevicesettings!=null)
            {
                foreach (var devicesetting in alldevicesettings)
                {

                    var DeviceName = devicesetting.DeviceID.ToString();

                    if (DeviceName == "151293")//OpenRGBDevice
                    {
                        var DeviceSerial = devicesetting.DeviceSerial;
                        kernel.Bind<IDeviceSpotSet>().To<DeviceSpotSet>().InSingletonScope().Named(DeviceSerial).WithConstructorArgument("deviceSettings", kernel.Get<IDeviceSettings>(DeviceSerial));
                        kernel.Bind<ISpotSetReader>().To<SpotSetReader>().InSingletonScope().Named(DeviceSerial).WithConstructorArgument("deviceSettings", kernel.Get<IDeviceSettings>(DeviceSerial)).WithConstructorArgument("deviceSpotSet", kernel.Get<IDeviceSpotSet>(DeviceSerial));                     
                        kernel.Bind<IStaticColor>().To<StaticColor>().InSingletonScope().Named(DeviceSerial).WithConstructorArgument("deviceSettings", kernel.Get<IDeviceSettings>(DeviceSerial)).WithConstructorArgument("deviceSpotSet", kernel.Get<IDeviceSpotSet>(DeviceSerial));
                        kernel.Bind<IRainbow>().To<Rainbow>().InSingletonScope().Named(DeviceSerial).WithConstructorArgument("deviceSettings", kernel.Get<IDeviceSettings>(DeviceSerial)).WithConstructorArgument("deviceSpotSet", kernel.Get<IDeviceSpotSet>(DeviceSerial));
                        kernel.Bind<IMusic>().To<Music>().InTransientScope().Named(DeviceSerial).WithConstructorArgument("deviceSettings", kernel.Get<IDeviceSettings>(DeviceSerial)).WithConstructorArgument("deviceSpotSet", kernel.Get<IDeviceSpotSet>(DeviceSerial));
                        kernel.Bind<IAtmosphere>().To<Atmosphere>().InSingletonScope().Named(DeviceSerial).WithConstructorArgument("deviceSettings", kernel.Get<IDeviceSettings>(DeviceSerial)).WithConstructorArgument("deviceSpotSet", kernel.Get<IDeviceSpotSet>(DeviceSerial));
                    }
                    else
                    {

                        if(!devicesetting.IsHUB)
                        {
                            kernel.Bind<IDeviceSpotSet>().To<DeviceSpotSet>().InSingletonScope().Named(DeviceName).WithConstructorArgument("deviceSettings", kernel.Get<IDeviceSettings>(DeviceName));
                            kernel.Bind<ISpotSetReader>().To<SpotSetReader>().InSingletonScope().Named(DeviceName).WithConstructorArgument("deviceSettings", kernel.Get<IDeviceSettings>(DeviceName)).WithConstructorArgument("deviceSpotSet", kernel.Get<IDeviceSpotSet>(DeviceName));
                            if(devicesetting.ParrentLocation==151293) // only attach serial stream to device with their own serial support
                            {
                                kernel.Bind<ISerialStream>().To<SerialStream>().InSingletonScope().Named(DeviceName).WithConstructorArgument("deviceSettings", kernel.Get<IDeviceSettings>(DeviceName)).WithConstructorArgument("deviceSpotSet", kernel.Get<IDeviceSpotSet>(DeviceName));
                                var serialStream = kernel.Get<ISerialStream>(DeviceName);
                            }
                                  
                            kernel.Bind<IStaticColor>().To<StaticColor>().InSingletonScope().Named(DeviceName).WithConstructorArgument("deviceSettings", kernel.Get<IDeviceSettings>(DeviceName)).WithConstructorArgument("deviceSpotSet", kernel.Get<IDeviceSpotSet>(DeviceName));
                            kernel.Bind<IRainbow>().To<Rainbow>().InSingletonScope().Named(DeviceName).WithConstructorArgument("deviceSettings", kernel.Get<IDeviceSettings>(DeviceName)).WithConstructorArgument("deviceSpotSet", kernel.Get<IDeviceSpotSet>(DeviceName));
                            kernel.Bind<IMusic>().To<Music>().InTransientScope().Named(DeviceName).WithConstructorArgument("deviceSettings", kernel.Get<IDeviceSettings>(DeviceName)).WithConstructorArgument("deviceSpotSet", kernel.Get<IDeviceSpotSet>(DeviceName));
                            kernel.Bind<IAtmosphere>().To<Atmosphere>().InSingletonScope().Named(DeviceName).WithConstructorArgument("deviceSettings", kernel.Get<IDeviceSettings>(DeviceName)).WithConstructorArgument("deviceSpotSet", kernel.Get<IDeviceSpotSet>(DeviceName));
                           
                            var spotSetReader = kernel.Get<ISpotSetReader>(DeviceName);
                            var staticColor = kernel.Get<IStaticColor>(DeviceName);
                            var rainbow = kernel.Get<IRainbow>(DeviceName);
                            var music = kernel.Get<IMusic>(DeviceName);
                            var atmosphere = kernel.Get<IAtmosphere>(DeviceName);
                        }

                  
                    }

                }

                foreach (var devicesetting in alldevicesettings)
                {
                    if (devicesetting.IsHUB)//hub object, bind all spotset
                    {
                        kernel.Bind<ISerialStream>().To<SerialStreamHUB>().InSingletonScope().Named(devicesetting.DeviceID.ToString()).WithConstructorArgument("deviceSettings", kernel.Get<IDeviceSettings>(devicesetting.DeviceID.ToString()));
                        var serialStream = kernel.Get<ISerialStream>(devicesetting.DeviceID.ToString());
                    }
                }

            }
            var alldevices = kernel.GetAll<IDeviceSettings>().ToList(); ;
          
            return kernel;
            // lighting viewmodel bây giờ chỉ có nhiệm vụ load data từ spotset và settings tương ứng với card sau đó display, không phải khởi tạo class như trước
            // sau bước này thì tất cả các service đều được khởi tạo, SerialStream, SpotSet và service tương ứng với SelectedEffect được khởi chạy//
            // điều này đảm bảo SpotSet có data để lightingviewmodel hiển thị, đồng thời SerialStream cũng lấy data đó gửi ra device
            // kể từ sau khi app khởi động, mọi event điều chỉnh giá trị ở MainViewViewModel sẽ bắn về class đang chạy (rainbow, Music...) và class
            // đó sẽ thay đổi ( những cái này trong từng class đã viết rồi), đồng thời settings được lưu

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
                foreach ( var serialStream in SerialStream)
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
                    var duplicatorreader = kernel.Get<IDesktopDuplicatorReader>();
                    var duplicatorreader2 = kernel.Get<IDesktopDuplicatorReaderSecondary>();
                    var duplicatorreader3 = kernel.Get<IDesktopDuplicatorReaderThird>();
                    duplicatorreader.RefreshCapturingState();
                    duplicatorreader2.RefreshCapturingState();
                    duplicatorreader3.RefreshCapturingState();

                    _log.Debug("Restart the serial stream after sleep!");
                }
                else if (e.Mode == PowerModes.Suspend)
                {
                    var SerialStream = kernel.GetAll<ISerialStream>();
                    foreach (var serialStream in SerialStream)
                    {
                        serialStream.Stop();
                    }
                    var duplicatorreader = kernel.Get<IDesktopDuplicatorReader>();
                    var duplicatorreader2 = kernel.Get<IDesktopDuplicatorReaderSecondary>();
                    var duplicatorreader3 = kernel.Get<IDesktopDuplicatorReaderThird>();
                    duplicatorreader.Stop();
                    duplicatorreader2.Stop();
                    duplicatorreader3.Stop();
                  

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
        MainView _newUIForm;
        private void OpenNewUI()
        {
            Current.MainWindow.WindowState = WindowState.Normal;
            Current.MainWindow.Show();
     
        }
      
        private void SetupNotifyIcon()

        {

            var icon = new System.Drawing.Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("adrilight.zoe.ico"));
            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            //contextMenu.Items.Add(new MenuItem("Dashboard", (s, e) => OpenNewUI()));
          //  contextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("Cài đặt...", (s, e) => OpenSettingsWindow()));
            //contextMenu.Items.Add(new MenuItem("Thoát", (s, e) => Shutdown(0)));

            //This Commented due to Net5 incompatible

            var notifyIcon = new System.Windows.Forms.NotifyIcon()
            {
                Text = $"adrilight {VersionNumber}",
                Icon = icon,
                Visible = true,
                ContextMenuStrip = contextMenu
            };
            //  notifyIcon.DoubleClick += (s, e) => { OpenSettingsWindow(); };
            notifyIcon.DoubleClick += (s, e) => { OpenNewUI(); };
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
