using BO;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace adrilight.ViewModel
{
    public class UserIncreamentInputViewModel : ViewModelBase
    {
        private int _startIndex;
        public int StartIndex {
            get { return _startIndex; }
            set
            {
                if (_startIndex == value) return;
                _startIndex = value;
                RaisePropertyChanged();
            }
        }
        private int _spacing;
        public int Spacing {
            get { return _spacing; }
            set
            {
                if (_spacing == value) return;
                _spacing = value;
                RaisePropertyChanged();
            }
        }


        public UserIncreamentInputViewModel()
        {
            //Card = device;

            //DeleteCommand = new RelayCommand<string>((p) => {
            //    return true;
            //}, (p) =>
            //{
            //   // some action
            //});

        }

    }
}
