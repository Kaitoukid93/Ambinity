using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace adrilight
{
    public interface ISecondDesktopFrame : INotifyPropertyChanged
    {
        byte[] Frame { get; set; }
        void Stop();
        void RefreshCapturingState();
    }
}