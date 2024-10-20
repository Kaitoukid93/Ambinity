using System.Collections.Generic;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.ViewModels;
using Ambinity.Views.AmbinityStore;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Geography;
using AmbinityCore.Models.Lighting.Zone;
using CommunityToolkit.Mvvm.Input;
using Draw2D.Core;

namespace Ambinity.Views.LayoutEditor.RightPanel.PropertiesView;

public class ConfigurationHeaderViewModel : ViewModelBase
{
    private readonly IWindowService _windowService;
    private ICollectableItem _item;

    public ConfigurationHeaderViewModel(IWindowService windowService, AmbinityStoreItemExportViewModel exportViewModel)
    {
        _exportViewModel = exportViewModel;
        _windowService = windowService;
        ExportSelectedItemCommand = new RelayCommand(ExportSelectedItem);
    }

    public void Init(List<Figure> figures)
    {
      
        if (figures == null || figures.Count == 0)
        {
            Header = "Select an item to begin";
            Icon = "arrow_cursor_2__mouse_select_cursor";
        }

        //null
        else if (figures.Count == 1)
        {
            if(figures[0] is not ContainerFigure)
                return;
            var childItem = (figures[0] as ContainerFigure).ChildItem;
            switch (childItem)
            {
                case LightingZone zone:
                    _item = zone;
                    Header = zone.Shape.ToString() + " - " + zone.LightingConfiguration.Name;
                    Icon = zone.Icon;
                    break;
                case AmbinityDevice device:
                    _item = device.Layout;
                    Header = device.DeviceName;
                    Icon = "page_setting__page_setting_square_triangle_circle_line_combination_variation";
                    break;
            }
        }
        else
        {
            Header = figures.Count.ToString() + " items selected";
            Icon = "layers_1__design_layer_layers_pile_stack_align";
        }
    }

    private void ExportSelectedItem()
    {
        _exportViewModel.Init(_item);
        var window = _windowService.ShowWindow(_exportViewModel);
    }

    private string _header;

    public string Header
    {
        get => _header;
        set
        {
            _header = value;
            OnPropertyChanged();
        }
    }

    private string _icon;

    public string Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            OnPropertyChanged();
        }
    }

    private bool _visibility = true;
    private readonly AmbinityStoreItemExportViewModel _exportViewModel;

    public bool Visibility
    {
        get => _visibility;
        set
        {
            _visibility = value;
            OnPropertyChanged();
        }
    }

    public ICommand ExportSelectedItemCommand { get; }
}