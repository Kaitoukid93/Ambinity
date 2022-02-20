using adrilight.Spots;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight
{
    public interface IGroupSettings : INotifyPropertyChanged
    {
        int GroupSelectedEffect { get; set; }
        int GroupSelectedPalette { get; set; }
        int GroupSelectedEffectSpeed { get; set; }
        string GroupName { get; set; }
        int GroupID { get; set; }
        string[] ChildsName { get; set; }

        int[] ChildsIndex { get; set; }
    }
}
