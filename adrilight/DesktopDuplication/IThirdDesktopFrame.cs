using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace adrilight
{
    public interface IThirdDesktopFrame : INotifyPropertyChanged
    {
        byte[] Frame { get; set; }
        void Stop();
        int FrameWidth { get; set; }
        int FrameHeight { get; set; }
        void RefreshCapturingState();
    }
}