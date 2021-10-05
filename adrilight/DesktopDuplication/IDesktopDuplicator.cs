using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight
{
    public interface IDesktopDuplicator
    {
        byte[] DesktopFrame { get; set; }

    }
}
