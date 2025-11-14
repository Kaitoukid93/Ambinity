using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ambinity.ViewModels;
using Ambinity.Views.Screens;
using AmbinityCore.Models;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;

namespace Ambinity.Views.SideMenu;

public class SideMenuScreenViewModel : ObservableObject
{
    public SideMenuScreenViewModel(string content, string  icon, ScreenViewModelBase screen)
    {
        Content = content;
        Screen = screen;
        Icon = icon;
    }

    private string _content = "Page";

    /// <summary>
    /// display text content of this item
    /// </summary>
    public string Content
    {
        get => _content;
        set
        {
            _content = value;
            OnPropertyChanged();
        }
    }

    private string _icon = "Boost";

    /// <summary>
    /// display icon of this item get from GeometryCollection.axaml
    /// </summary>
    public string Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            OnPropertyChanged();
        }
    }
    private ScreenViewModelBase _screen;

    /// <summary>
    /// Viewmodel to navigate to when this item is selected
    /// </summary>
    public ScreenViewModelBase Screen
    {
        get => _screen;
        set
        {
            _screen = value;
            OnPropertyChanged();
        }
    }

}
