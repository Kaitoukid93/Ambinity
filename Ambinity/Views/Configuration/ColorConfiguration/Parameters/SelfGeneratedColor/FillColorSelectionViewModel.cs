using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.Views.AmbinityStore;
using Ambinity.Views.LayoutEditor;
using Ambinity.Windows;
using AmbinityCore.Colors;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using AmbinityCore.Repositories;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Serilog;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class FillColorSelectionViewModel : ParameterViewModelBase
{
    public FillColorSelectionViewModel(SelfGeneratedColorConfiguration configuration,
        ProfileEditorRightPanelViewModel rightPanelViewModel, StaticColorsRepository staticColorsRepository, ColorPaletteRepository colorPaletteRepository,
        LibraryViewModelFactory libraryViewModelFactory, IWindowService windowService, IDialogService dialogService, AmbinityStoreItemExportViewModel itemExportViewModel)
    {
        _itemExportViewModel = itemExportViewModel;
        _libraryViewModelFactory = libraryViewModelFactory;
        _colorsRepository = staticColorsRepository;
        _configuration = configuration;
        _windowService = windowService;
        _dialogService = dialogService;
        _colorPaletteRepository = colorPaletteRepository;
        Colors = new ObservableCollection<SolidColorViewModel>();
        foreach (var color in configuration.Colors)
        {
            var col = new SolidColorViewModel(color);
            RegisterColor(col);
            Colors.Add(col);
        }

        ImportPaletteCommand = new AsyncRelayCommand(ImportPalette);
        ExportPaletteCommand = new RelayCommand(ExportPalette);
        AddPaletteToLibraryCommand = new AsyncRelayCommand(AddPaletteToLibrary);
        SelectedColors = new ObservableCollection<SolidColorViewModel>();
        AddColorCommand = new RelayCommand<Color>(AddColor);
        OpenLibraryCommand = new AsyncRelayCommand(OpenLibrary);
        _rightPanelViewModel = rightPanelViewModel;
        
    }

    private async Task AddPaletteToLibrary()
    {
        var vm = new InputDialogContentViewModel();
        vm.DialogClosed += OnCreateNewProfileDialogClosed;
        await _dialogService.ShowInputDialog(vm, "New profile", "Ok", "Cancel");
    }

    private void OnCreateNewProfileDialogClosed(object? sender, EventArgs e)
    {
        var vm = sender as InputDialogContentViewModel;
        var result = (e as ContentDialogClosedEventArgs).Result;
        if (result == ContentDialogResult.Secondary || result == ContentDialogResult.None)
            return;
        if (result == ContentDialogResult.Primary)
        {
            //create new profile
            var palette = new ColorPalette(vm.UserInput,_configuration.Colors.ToArray());
            _colorPaletteRepository.AddItem(palette);
        }
    }

    private void  ExportPalette()
    {
        var palette = new ColorPalette("New Color Palette", _configuration.Colors.ToArray());
        _itemExportViewModel.Init(palette);
        var window = _windowService.ShowWindow(_itemExportViewModel);
    }


    private void OnPaletteSelected(AssetItemViewModelBase item)
    {
        if (item is ColorPaletteAssetViewModel)
        {
            var colorPaletteAsset = item as ColorPaletteAssetViewModel;
            ApplyPalette(colorPaletteAsset.Item as ColorPalette);
        }
     
    }

    private async Task ImportPalette()
    {
        string[]? result = await _windowService.CreateOpenFileDialog()
            .HavingFilter(f => f.WithExtension("json").WithName("json file"))
            .ShowAsync();

        if (result == null)
            return;
        var importFilePath = result.First();
        var hexConverter = new HexColorConverter();
        var palette = JsonHelpers.DeserializeJson<ColorPalette>(importFilePath, hexConverter);
        if (palette == null)
        {
            Log.Error("Palette parse error: " + importFilePath);
            return;
        }

        ApplyPalette(palette);
    }

    private void ApplyPalette(ColorPalette colorPalette)
    {
        Colors?.Clear();
        _configuration.Colors.Clear();
        foreach (var color in colorPalette.Colors)
        {
            AddColor(color);
        }

        _configuration.UpdateColors();
    }

    private async Task OpenLibrary()
    {
        _libraryViewModel = _libraryViewModelFactory.GetLibraryViewModel("ColorPalette");
        _libraryViewModel?.Init();
        _libraryViewModel.ItemSelected += OnPaletteSelected;
        _rightPanelViewModel.OpenFlyout(_libraryViewModel);
    }

    public ObservableCollection<SolidColorViewModel> Colors { get; set; }
    private ObservableCollection<SolidColorViewModel> _selectedColors;
    private ProfileEditorRightPanelViewModel _rightPanelViewModel;
    private StaticColorsRepository _colorsRepository;
    private readonly SelfGeneratedColorConfiguration _configuration;
    private readonly LibraryViewModelFactory _libraryViewModelFactory;
    private LibraryViewModelBase _libraryViewModel;
    private readonly IWindowService _windowService;
    private readonly IDialogService _dialogService;
    private readonly ColorPaletteRepository _colorPaletteRepository;
    private readonly AmbinityStoreItemExportViewModel _itemExportViewModel;

    public ObservableCollection<SolidColorViewModel> SelectedColors
    {
        get => _selectedColors;
        set
        {
            _selectedColors = value;
            OnPropertyChanged();
        }
    }

    private void AddColor(Color color)
    {
        //add new solid red color to the colletion
        if (color == null)
            color = Color.FromRgb(255, 0, 0);
        var newCol = new SolidColorViewModel(color);
        RegisterColor(newCol);
        Colors.Insert(0, newCol);
        _configuration.Colors.Insert(0, newCol.Color);
        _configuration.UpdateColors();
    }

    private void InsertColor(int index)
    {
        var newCol = new SolidColorViewModel(Color.FromRgb(255, 0, 0));
        RegisterColor(newCol);
        Colors.Insert(index, newCol);
        _configuration.Colors.Insert(index, newCol.Color);
        _configuration.UpdateColors();
    }

    private void RegisterColor(SolidColorViewModel color)
    {
        color.SelfRemoved += OnColorRemoved;
        color.ColorPickerFlyoutRequest += OnColorPickerFlyoutRequest;
        color.ColorChanged += OnColorChanged;
        color.InsertAboveEvent += OnInsertAbove;
        color.InsertBelowEvent += OnInsertBelow;
    }

    private void OnInsertBelow(SolidColorViewModel obj)
    {
        var index = Colors.IndexOf(obj);
        InsertColor(index + 1);
    }

    private void OnInsertAbove(SolidColorViewModel obj)
    {
        var index = Colors.IndexOf(obj);
        InsertColor(index);
    }

    private void UnregisterColor(SolidColorViewModel color)
    {
        color.SelfRemoved -= OnColorRemoved;
        color.ColorPickerFlyoutRequest -= OnColorPickerFlyoutRequest;
        color.ColorChanged -= OnColorChanged;
    }

    public override void Dispose()
    {
        if (_libraryViewModel != null)
            _libraryViewModel.ItemSelected -= OnPaletteSelected;
        foreach (var color in Colors)
        {
            UnregisterColor(color);
        }

        Colors.Clear();
    }

    private void OnColorChanged(object? sender, EventArgs e)
    {
        var index = Colors.IndexOf(sender as SolidColorViewModel);
        _configuration.Colors[index] = (sender as SolidColorViewModel).Color;
        _configuration.UpdateColors();
    }

    private void OnColorPickerFlyoutRequest(object? sender, EventArgs e)
    {
        //logic is a mess here, sorry
        //null check
        if (SelectedColors == null)
            SelectedColors = new ObservableCollection<SolidColorViewModel>();
        //if clicked color is not in the selected list, dumb guy do that, but we have to do it anyway
        var clickedItem = sender as SolidColorViewModel;
        if (!SelectedColors.Contains(clickedItem))
        {
            SelectedColors = null;
            SelectedColors = new ObservableCollection<SolidColorViewModel>();
            SelectedColors.Add(clickedItem);
        }

        var colorPickerViewModel = new SolidColorPickerViewModel(SelectedColors.ToList(), _colorsRepository);
        _rightPanelViewModel.OpenFlyout(colorPickerViewModel);
    }

    private void OnColorRemoved(object? sender, EventArgs e)
    {
        if (SelectedColors.Count == 0)
        {
            SelectedColors.Add(sender as SolidColorViewModel);
        }

        foreach (var color in SelectedColors.ToList())
        {
            var index = Colors.IndexOf(color);
            UnregisterColor(color);
            Colors.Remove(color);
            _configuration.Colors.RemoveAt(index);
        }

        _configuration.UpdateColors();
    }

    public ICommand AddColorCommand { get; set; }
    public ICommand OpenLibraryCommand { get; set; }
    public ICommand ImportPaletteCommand { get; }
    public ICommand ExportPaletteCommand { get; }
    public ICommand AddPaletteToLibraryCommand { get; }
}