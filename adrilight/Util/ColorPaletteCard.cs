using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace adrilight.Util
{
    internal class ColorPaletteCard : IColorPaletteCard
    {
        public ColorPaletteCard(string name, string owner, string type, string description, System.Windows.Media.Color[] thumbnail )
        {
            Name = name;
            Owner = owner;
            Type = type;
            Description = description;
            Thumbnail = thumbnail;


        }

    public string Name { get; set; }
    public string Owner { get; set; }
    public System.Windows.Media.Color[] Thumbnail { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    }
}
