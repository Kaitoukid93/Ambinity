using adrilight.Util;
using adrilight.ViewModel;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for AddDevice.xaml
    /// </summary>
    public partial class AddDevice : UserControl
    {
        public AddDevice()
        {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isValid = false;
            if (ComportBox.SelectedItem!=null)
            isValid = CheckSerialPort(ComportBox.SelectedItem.ToString());
            if (!isValid)
                ComportBox.SelectedItem = null;
        }
        public List<IDeviceSettings> _selectedFans=new List<IDeviceSettings>();
        public List<IDeviceSettings> _selectedChild = new List<IDeviceSettings>();


        private bool CheckSerialPort(string serialport)
        {
           
            var available = true;
            int TestbaudRate = 1000000;

            if (serialport != null)
            {
                if (serialport == "Không có")
                {
                    // System.Windows.MessageBox.Show("Serial Port " + serialport + " is just for testing effects, not the real device, please note");
                    available = true;
                    return available;

                }
                var serialPorttest = (ISerialPortWrapper)new WrappedSerialPort(new SerialPort(serialport, TestbaudRate));

                try
                {

                    serialPorttest.Open();

                }

                catch (Exception)
                {

                    // BlockedComport.Add(serialport);
                   // _log.Debug("Serial Port " + serialport + " access denied, added to Blacklist");
                    HandyControl.Controls.MessageBox.Show( serialport + " đã được sử dụng hoặc không hợp lệ , vui lòng chọn COM khác ", "Serial Port", MessageBoxButton.OK, MessageBoxImage.Error);
                    available = false;

                    //_log.Debug(ex, "Exception catched.");
                    //to be safe, we reset the serial port
                    //  MessageBox.Show("Serial Port " + UserSettings.ComPort + " is in use or unavailable, Please chose another COM Port");




                    //allow the system some time to recover

                    // Dispose();
                }
                serialPorttest.Close();

            }

            else
            {
                available = false;
            }

            return available;


        }

        private void FanSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedFans != null)
                _selectedFans.Clear();
            foreach(var item in FanSelection.SelectedItems)
            {
                _selectedFans.Add(item as IDeviceSettings);
            }
            
            var vm = this.DataContext as AddDeviceViewModel;
            vm.SelectedOutputs = _selectedFans;
        }
        private void GroupChildSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedChild != null)
                _selectedChild.Clear();
            foreach (var item in GroupChildSelection.SelectedItems)
            {
                _selectedChild.Add(item as IDeviceSettings);
            }

            var vm = this.DataContext as AddDeviceViewModel;
            vm.SelectedChilds = _selectedChild;
        }
    }
}
