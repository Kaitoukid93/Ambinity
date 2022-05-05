using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Settings
{
    internal class DeviceProfile : IDeviceProfile
    {
        public string Name { get; set; }
        public string DeviceType { get; set; }
        public string Owner { get; set; }
        public string Description { get; set; }
        public string Geometry { get; set; }
        public string ProfileUID { get; set; }
        public IOutputSettings[] OutputSettings { get; set; }
    }
}
