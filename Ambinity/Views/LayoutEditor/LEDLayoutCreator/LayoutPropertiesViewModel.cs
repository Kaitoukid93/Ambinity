using System;
using System.IO;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.ViewModels;
using Ambinity.Views.LayoutEditor.LEDLayoutCreator;
using AmbinityCore;
using AmbinityServer.OnlineItem;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SkiaSharp;
using System.Collections.Generic;
namespace Ambinity.Views.LayoutEditor.LEDLayoutCreator;

public class LayoutPropertiesViewModel : ViewModelBase
{
    public LayoutPropertiesViewModel()
    {
        _windowService = Ioc.Default.GetRequiredService<IWindowService>();
        _thumbnailService = Ioc.Default.GetRequiredService<ThumbnailService>();
        _layoutImageEditorViewModel = Ioc.Default.GetRequiredService<LayoutImageEditorViewModel>();
        _layoutImageEditorViewModel.OnUserAccept += ImageEditAccepted;
        SelectDeviceImageCommand = new AsyncRelayCommand(SelectDeviceImage);
        EditImageCommand = new AsyncRelayCommand(EditSelectedImage);
    }

    private IWindowService _windowService;
    private ThumbnailService _thumbnailService;
    private LayoutImageEditorViewModel _layoutImageEditorViewModel;
    private Window _imageEditorWindow;
    public Window HostWindow { get; set; }
    public ICommand SelectDeviceImageCommand { get; set; }
    public ICommand EditImageCommand { get; set; }

    private float _width = 120;
    private float _height = 120;
    public float Width
    {
        get => _width;
        set
        {
            _width = value;
            OnPropertyChanged();
        }
    }
    public float Height
    {
        get => _height;
        set
        {
            _height = value;
            OnPropertyChanged();
        }
    }

    private string? _imagePath;
    public string? ImagePath
    {
        get => _imagePath;
        set
        {
            _imagePath = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(GetThumbnail));
        }
    }
    private string _editedImage => Path.Combine(Constants.CacheFolderPath, "LayoutCreator", "thumb_edited.png");
     private string _originalImage => Path.Combine(Constants.CacheFolderPath, "LayoutCreator", "thumb.png");

    /// <summary>
    /// Open separate window to edit selected image,
    /// todo: keep selected original image in cache
    /// </summary>
    /// <returns></returns>
    private async Task EditSelectedImage()
    {
        ///edit selected image and create a temp folder to put everything that related to
        /// editing process inside it
        if (ImagePath == null)
            return;
        _layoutImageEditorViewModel.Init(ImageWidth, ImageHeight, _originalImage, _editedImage);
        if (_imageEditorWindow != null && _imageEditorWindow.IsVisible)
        {
            _imageEditorWindow.Activate();
            return;
        }

        if (ImagePath == null)
            return;
        _imageEditorWindow = await _windowService.ShowDialogWindow(_layoutImageEditorViewModel, HostWindow);
    }

    private void ImageEditAccepted()
    {
        ImagePath = _editedImage;
       // OnPropertyChanged(nameof(GetThumbnail));
    }

    private async Task SelectDeviceImage()
    {
        if (_imageEditorWindow != null && _imageEditorWindow.IsVisible)
        {
            _imageEditorWindow.Activate();
            return;
        }
        string[]? result = await _windowService.CreateOpenFileDialog()
             .WithTitle("Select Device Image")
             .HavingFilter(f => f.WithExtension("png").WithName("png file"))
             .WithDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures))
             .ShowAsync();
        if (result == null || result.Length == 0)
            return;
        await InitializeCache(result[0]);
        InitializeImage();
    }
    /// <summary>
    /// Initialize cache for editing and exporting device layout
    /// </summary>
    private async Task InitializeCache(string imagePath)
    {
        //create cache folder /cache/LayoutCreator
        //copy image to cache
        var cachePath = Path.Combine(Constants.CacheFolderPath, "LayoutCreator");
        Directory.CreateDirectory(cachePath);
        File.Copy(imagePath, _originalImage, true);
        ImagePath = _originalImage;
        //OnPropertyChanged(nameof(GetThumbnail));
    }
    public Task<Bitmap?> GetThumbnail => GetThumbnailAsync();
    private async Task<Bitmap?> GetThumbnailAsync()
    {

        if (!File.Exists(ImagePath))
            return null;
        var thumb = await _thumbnailService.LoadThumbnail(ImagePath, 200, true);
        return thumb;
    }

    /// <summary>
    /// Scan input image for error and size mismatch
    /// </summary>
    public SKBitmap Bitmap { get; private set; }
    public int ImageWidth { get; private set; }
    public int ImageHeight { get; private set; }
    private void InitializeImage()
    {
        if (string.IsNullOrEmpty(ImagePath) || !File.Exists(ImagePath))
            throw new FileNotFoundException("Image path is invalid.", ImagePath);

        using var stream = File.OpenRead(ImagePath);
        using var skiaStream = new SKManagedStream(stream);
        var bitmap = SKBitmap.Decode(skiaStream);

        if (bitmap == null)
            throw new InvalidDataException("Failed to decode image.");

        Bitmap = bitmap;
        ImageWidth = bitmap.Width;
        ImageHeight = bitmap.Height;
        ShowEditImageButton = true;

    }
    private bool _showEditImageButton;
    public bool ShowEditImageButton
    {
        get => _showEditImageButton;
        set
        {
            _showEditImageButton = value;
            OnPropertyChanged();
        }
    }

    public string SelectedLEDShape { get; set; } = "Square";
    public List<string> LEDShapes { get; set; } = new List<string>
    {
        "Square",
        "Circle"
    };
    public void Init(float width = 500, float height = 500)
    {
        Width = width;
        Height = height;
        LayoutName = "New Device Layout";
        LEDCount = 20;
    }
    public string LayoutName { get; set; } = "New Device Layout";
    public int LEDCount { get; set; }


}
