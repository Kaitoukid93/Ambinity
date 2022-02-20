using adrilight.Spots;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight
{
    internal class GroupSettings : ViewModelBase, IGroupSettings
    {
       //EffectSetting
        private int _groupSelectedEffect = 0;
        private int _groupSelectedPalette = 0;
        private int _groupSelectedEffectSpeed = 1;
        private string _groupName = "CaseLED";
        private int _groupID = 0;
        private string[] _childsName= new string [256];
        private int[] _childsIndex = new int[256];


       


        public int GroupSelectedEffect { get => _groupSelectedEffect; set { Set(() => GroupSelectedEffect, ref _groupSelectedEffect, value); } }
        public int GroupSelectedPalette { get => _groupSelectedPalette; set { Set(() => GroupSelectedPalette, ref _groupSelectedPalette, value); } }
        public int GroupSelectedEffectSpeed { get => _groupSelectedEffectSpeed; set { Set(() => GroupSelectedEffectSpeed, ref _groupSelectedEffectSpeed, value); } }
        public string GroupName { get => _groupName; set { Set(() => GroupName, ref _groupName, value); } }
        public int GroupID { get => _groupID; set { Set(() => GroupID, ref _groupID, value); } }
        public string[] ChildsName { get => _childsName; set { Set(() => ChildsName, ref _childsName, value); } }
        public int[] ChildsIndex { get => _childsIndex; set { Set(() => ChildsIndex, ref _childsIndex, value); } }



    }
}
