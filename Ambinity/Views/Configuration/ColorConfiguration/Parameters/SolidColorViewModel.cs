using System;
using System.Globalization;
using System.Windows.Input;
using Ambinity.ViewModels;
using Ambinity.Views.LayoutEditor;
using AmbinityCore.Repositories;
using Avalonia.Media;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class SolidColorViewModel : ViewModelBase
{
    public event EventHandler SelfRemoved;
    public event EventHandler ColorPickerFlyoutRequest;
    public event EventHandler ColorChanged;
    public event Action<SolidColorViewModel> InsertAboveEvent;
    public event Action<SolidColorViewModel> InsertBelowEvent;
    public SolidColorViewModel(Color color)
    {
        Color = color;
        RemoveCommand = new RelayCommand(Remove);
        OpenColorPickerCommand = new RelayCommand(OpenColorPickerFlyout);
        InsertAboveCommand = new RelayCommand(InsertAbove);
        InsertBelowCommand = new RelayCommand(InsertBelow);

    }

    private void InsertBelow()
    {
        InsertBelowEvent?.Invoke(this);
    }

    private void InsertAbove()
    {
        InsertAboveEvent?.Invoke(this);
        
    }


    private void OpenColorPickerFlyout()
    {
        ColorPickerFlyoutRequest?.Invoke(this, EventArgs.Empty);
    }
    
    private Color _color;

    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            var ui = value.ToUInt32();
            HexColor = ui.ToString("x8", CultureInfo.InvariantCulture).Remove(0,2);
            OnPropertyChanged();
            ColorChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private string _hexColor;
    public string HexColor
    {
        get => _hexColor;
        set
        {
            
            _hexColor = value;
            OnPropertyChanged();
        }
    }
    private void Remove()
    {
        SelfRemoved?.Invoke(this, EventArgs.Empty);
    }
    public ICommand RemoveCommand { get; set; }
    public ICommand OpenColorPickerCommand { get; set; }
    public ICommand InsertAboveCommand { get; set; }
    public ICommand InsertBelowCommand { get; set; }
}