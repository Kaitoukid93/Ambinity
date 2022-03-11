using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Util
{
    public interface IColorPaletteCard 
    {
        string Name { get; set; }
        string Owner { get; set; }
        System.Windows.Media.Color[] Thumbnail { get; set; }
        string Type { get; set; }
        string Description { get; set; }
    }
}
