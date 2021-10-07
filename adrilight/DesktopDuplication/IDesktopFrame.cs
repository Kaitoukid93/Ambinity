using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace adrilight
{
    public interface IDesktopFrame : INotifyPropertyChanged
    {
        byte[] Frame { get; set; }
    }
}