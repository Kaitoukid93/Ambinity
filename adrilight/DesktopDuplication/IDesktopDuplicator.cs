using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight
{
    public interface IDesktopDuplicator : INotifyPropertyChanged
    {
        byte[] DesktopFrame { get; set; }

    }
}
