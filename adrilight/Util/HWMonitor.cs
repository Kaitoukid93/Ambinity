using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Controls;
using System.Threading;
using Castle.Core.Logging;
using NLog;
using adrilight.ViewModel;
using System.Diagnostics;
using adrilight.Spots;
using LibreHardwareMonitor.Hardware;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;

namespace adrilight.Util
{
    internal class HWMonitor : ViewModelBase, IHWMonitor
    {


        private readonly NLog.ILogger _log = LogManager.GetCurrentClassLogger();


        public HWMonitor(IGeneralSettings generalSettings, MainViewViewModel mainViewViewModel)
        {

            MainViewViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));

            RefreshHWState();
            _log.Info($"Hardware Monitor Created");

        }
        IComputer thisComputer { get; set; }
        public int AutoFanSpeed { get; set; }
        LibreHardwareMonitor.Hardware.Computer computer { get; set; }
        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //switch (e.PropertyName)
            //{
            //    case nameof(OutputSettings.OutputIsEnabled):
            //    case nameof(OutputSettings.OutputSelectedMode):

            //        RefreshColorState();
            //        break;
            //    case nameof(OutputSettings.OutputStaticColor):
            //    case nameof(OutputSettings.OutputSelectedGradient):
            //        SolidColorChanged();
            //        break;

            //}
        }

        //DependencyInjection//
        private UpdateVisitor updateVisitor = new UpdateVisitor();
        private IGeneralSettings GeneralSettings { get; }

        private MainViewViewModel MainViewViewModel { get; }
        private Computer displayHWInfo { get; set; }

        public bool IsRunning { get; private set; } = false;
        private CancellationTokenSource _cancellationTokenSource;
        private void RefreshHWState()
        {
            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = true;
            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                _log.Debug("stopping the HWMonitor");
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
            }


            else if (!isRunning && shouldBeRunning)
            {
                //start it
                Init();
                _log.Debug("starting the HWMonitor");
                _cancellationTokenSource = new CancellationTokenSource();
                var thread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "HWMonitor"
                };
                thread.Start();
            }
        }


        public void Run(CancellationToken token)//static color creator
        {

            if (IsRunning) throw new Exception(" HWMonitor is already running!");

            IsRunning = true;

            _log.Debug("Started HW Monitor.");


            //init new hardware and sensor for dispayHWInfo


            try
            {

                thisComputer = new Computer();
                thisComputer.Processor = new List<IHardware>(); // init cpu list
                thisComputer.MotherBoard = new List<IHardware>(); // init mb list
                thisComputer.Ram = new List<IHardware>(); // init mb list
                thisComputer.GraphicCard = new List<IHardware>(); // init mb list
                foreach (var hardware in computer.Hardware)
                {
                    if (hardware.HardwareType == HardwareType.Cpu)
                        thisComputer.Processor.Add(hardware);
                    if (hardware.HardwareType == HardwareType.Motherboard)
                        thisComputer.MotherBoard.Add(hardware);
                    if (hardware.HardwareType == HardwareType.Memory)
                        thisComputer.Ram.Add(hardware);
                    if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd)
                        thisComputer.GraphicCard.Add(hardware);
                }

                while (!token.IsCancellationRequested)
                {

                    //get median fan value


                    // decide if it's necessary to update to the fan
                    //formular is if the fan speed changed more than 10% compare to last fan speed, apply the change

                    computer.Accept(updateVisitor);

                 
                    Thread.Sleep(1000);
                }
                // update every second








            }
            //motion speed




            catch (OperationCanceledException)
            {
                _log.Debug("OperationCanceledException catched. returning.");

                return;
            }
            catch (Exception ex)
            {
                _log.Debug(ex, "Exception catched.");

                Thread.Sleep(500);
            }
            finally
            {

                computer.Close();
                _log.Debug("Stopped HW Monitoring!!!");
                IsRunning = false;
            }

        }

        public void Init()
        {
            computer = new LibreHardwareMonitor.Hardware.Computer {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true,
                IsNetworkEnabled = true,
                IsStorageEnabled = true

            };

            computer.Open();
            computer.Accept(updateVisitor);
            displayHWInfo = new Computer();

        }

        public void Dispose()
        {
            computer.Close();
        }





    }
}