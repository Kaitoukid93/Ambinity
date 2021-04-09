﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BO
{
  public  class VerticalMenuItem: INotifyPropertyChanged
    {
        private bool _isActice = false;
        public bool IsActive
        {
            get { return _isActice; }
            set
            {
                if (_isActice == value) return;
                _isActice = value;
                OnPropertyChanged();
            }
        }
        private string _text;
        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value) return;
                _text = value;
                OnPropertyChanged();
            }
        }
        private string _images;
        public string Images
        {
            get { return _images; }
            set
            {
                if (_images == value) return;
                _images = value;
                OnPropertyChanged();
            }
        }
        private ICommand _action;
        public ICommand Action
        {
            get { return _action; }
            set
            {
                if (_action == value) return;
                _action = value;
                OnPropertyChanged();
            }
        }

        private bool _isVisible = true;
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible == value) return;
                _isVisible = value;
                OnPropertyChanged();
            }
        }
        private MenuButtonType _type = MenuButtonType.General;
        public MenuButtonType Type
        {
            get { return _type; }
            set
            {
                if (_type == value) return;
                _type = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public enum MenuButtonType : byte
    {
        General = 0,
        Dashboard = 1
    }
}
