using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight.Settings
{
    public interface IDeviceProfile
    {
        string Name { get; set; }
        string DeviceType { get; set; }
        string Owner { get; set; }
        string Description { get; set; }
        string Geometry { get; set; }
        string ProfileUID { get; set; }
        IOutputSettings[] OutputSettings { get; set; }
    }
}
