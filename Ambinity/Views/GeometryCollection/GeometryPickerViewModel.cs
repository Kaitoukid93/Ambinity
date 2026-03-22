using System.Collections.Generic;
using Ambinity.Views.SideMenu;
using Ambinity.Windows;
using AmbinityCore.Models.Profile;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;

namespace Ambinity.Views;

public class GeometryPickerViewModel : WindowDialogViewModelBase
{
    public List<string> AvailableIcons { get; set; }
    private ProfilePropertiesEditorViewModel _editorViewModel;
    public GeometryPickerViewModel(ProfilePropertiesEditorViewModel editorViewModel)
    {
        Color = Colors.White;
        _editorViewModel = editorViewModel;
        AvailableIcons = new List<string>();
        var dicts = Application.Current.Resources.MergedDictionaries;
        var geometriesDict = dicts[1] as ResourceDictionary;
        foreach (var key in geometriesDict.Keys)
        {
            AvailableIcons.Add(key as string);
        }
        //todo geometry viewmodel
    }
    public string SelectedGeometry { get; set; }
    public Color Color { get; set; }

    public override void DialogOnClosedAsync(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        Dialog.Closed -= DialogOnClosedAsync;
        var result = args.Result;
        if (result != ContentDialogResult.Primary)
            return;
        //save data here and notify icon change
        _editorViewModel.IconType = IconTypeEnum.Geometry;
        _editorViewModel.Icon = SelectedGeometry!=null? SelectedGeometry: "nullIcon";
        _editorViewModel.IconColor = Color;
        DialogClosed?.Invoke(this, args);
    }
}
