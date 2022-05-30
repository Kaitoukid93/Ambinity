using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using adrilight.DesktopDuplication;

namespace adrilight
{
    public interface IThirdDesktopFrame : INotifyPropertyChanged
    {
        ByteFrame Frame { get; set; }
        void Stop();
  
        void RefreshCapturingState();
    }
}