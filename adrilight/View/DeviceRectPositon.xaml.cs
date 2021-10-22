using System;
using System.Collections.Generic;
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
    /// Interaction logic for DeviceRectPositon.xaml
    /// </summary>
    public partial class DeviceRectPositon : UserControl
    {
        public DeviceRectPositon()
        {
            InitializeComponent();
        }

        private void DeviceCanvas_DragOver(object sender, DragEventArgs e)
        {
            
            Point dropPosition = e.GetPosition(deviceCanvas);
            if (dropPosition.X + deviceRect.Width > deviceCanvas.Width)
                dropPosition.X = deviceCanvas.Width - deviceRect.Width;
            if (dropPosition.Y + deviceRect.Height > deviceCanvas.Height)
                dropPosition.Y = deviceCanvas.Height - deviceRect.Height;
            Canvas.SetLeft(deviceRect, dropPosition.X);
            Canvas.SetTop(deviceRect, dropPosition.Y);
        }

        private void DeviceCanvas_Drop(object sender, DragEventArgs e)
        {
            //var vm = this.DataContext as MainViewViewModel;

            ////Call command from viewmodel     
            //if ((vm != null) && (vm.DeviceRectDropCommand.CanExecute(null)))
            //    vm.DeviceRectDropCommand.Execute(null);
        }

        private void DeviceRect_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(deviceRect, deviceRect, DragDropEffects.Move);
            }
        }
    }
}
