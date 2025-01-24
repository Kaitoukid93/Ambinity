using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.Views.Draw2DCanvas;
using AmbinityCore.CapturingService;
using AmbinityCore.DataBase;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using AmbinityCore.Models.Profile;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.Input;
using HPPH;
using ScreenCapture.NET;
using Serilog;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class ScreenRegionSelectionParameterViewModel : ParameterViewModelBase
{
    private readonly IWindowService _windowService;
    private ScreenCaptureRegionSelectionViewModel _regionSelectionViewModel;
    private readonly GeneralSettingsManager _settingsManager;
    private readonly ScreenCaptureConfiguration _config;
    private ICaptureZone _captureZone;
    private IScreenCapture _screenCapture;
    private byte[] _reusableRow;
    private bool _shouldShowImage;
    private readonly LightingProfileDecoder _decoder;
    public event Action PreviewImageUpdated;

    public ScreenRegionSelectionParameterViewModel(ScreenCaptureConfiguration config, IWindowService windowService,
        GeneralSettingsManager settingsManager, ScreenCapturingService capturingService, LightingProfileDecoder decoder)
    {
        _decoder = decoder;
        _config = config;
        _windowService = windowService;
        _settingsManager = settingsManager;
        OpenRegionSelectionCommand = new RelayCommand(OpenScreenRegionSelection);
        _capturingService = capturingService;
        _screenCapture = _capturingService.IsEnabled ? _capturingService.GetScreenCapture(_config.DisplayIndex) : null;
        if (_screenCapture == null)
        {
            ErrorMessage = "ScreenCapturingService for this display is not available";
            return;
        }
        _capturingService.FrameUpdated += OnFrameUpdate;
        _decoder.RenderingStatusChanged += OnRenderingStatusChanged;
        OnRenderingStatusChanged();
        AvailableScreen = new List<ScreenDataDisplay>();
        foreach (var screen in _capturingService.AvailableScreens)
        {
            var dataDisplay = new ScreenDataDisplay("Display " + (screen.Index + 1) + "-" + screen.GraphicsCard.Name,
                screen.Index);
            AvailableScreen.Add(dataDisplay);
        }
        //todo move init to background task
        Init();
    }

    private void OnRenderingStatusChanged()
    {
        if (_decoder.IsRendering)
            EnableEdit = false;
        else
        {
            EnableEdit = true;
        }
    }

    private bool _enableEdit;

    public bool EnableEdit
    {
        get => _enableEdit;
        set
        {
            _enableEdit = value;
            OnPropertyChanged();
        }
    }

    private void Init()
    {
        _shouldShowImage = true;
        _capturingService.RegisterUse();
        if (_captureZone != null)
        {
            try
            {
                _screenCapture?.UnregisterCaptureZone(_captureZone);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
        try
        {
            var left = _config.ScreenCaptureArea.RatioX * _screenCapture.Display.Width;
            var top = _config.ScreenCaptureArea.RatioY * _screenCapture.Display.Height;
            var width = _config.ScreenCaptureArea.RatioWidth * _screenCapture.Display.Width;
            var height = _config.ScreenCaptureArea.RatioHeight * _screenCapture.Display.Height;
            _captureZone = _screenCapture.RegisterCaptureZone((int)left, (int)top, (int)width,
                (int)height, downscaleLevel: 3);
            _reusableRow = new byte[(int)_captureZone.Width * 4];
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
        }
    }

    private void OpenScreenRegionSelection()
    {
        //get sregion property from config
        IsEnabled = false;
        _canvasViewModel = new Draw2DCanvasViewModel(_settingsManager);
        _regionSelectionViewModel =
            new ScreenCaptureRegionSelectionViewModel(_canvasViewModel, _config, AvailableScreen, _windowService);
        _regionSelectionViewModel.CloseMe += CloseRegionSelectionWindow;
    }

    private void CloseRegionSelectionWindow()
    {
        Init();
        _regionSelectionViewModel.Dispose();
        IsEnabled = true;
    }

    private Draw2DCanvasViewModel _canvasViewModel;
    private ScreenCapturingService _capturingService;
    public List<ScreenDataDisplay> AvailableScreen { get; set; }

    public ICommand OpenRegionSelectionCommand { get; set; }

    public class ScreenDataDisplay
    {
        public ScreenDataDisplay(string displayName, int index)
        {
            Name = displayName;
            Index = index;
        }

        public string Name { get; set; }
        public int Index { get; set; }
    }

    private WriteableBitmap? _previewImage;

    public WriteableBitmap? PreviewImage
    {
        get => _previewImage;
        set
        {
            _previewImage = value;
            OnPropertyChanged();
        }
    }

    private WriteableBitmap _reusableBitmap;

    private void OnFrameUpdate(int index)
    {
        if (!_shouldShowImage)
            return;
        if (index != _config.DisplayIndex)
            return;
        if (_captureZone == null)
            return;
        using (_captureZone.Lock())
        {
            IImage image = _captureZone.Image;
            Span<byte> row = _reusableRow;
            // check if image dimesion is match
            if (_reusableBitmap == null || _reusableBitmap.Size.Width != image.Width ||
                _reusableBitmap.Size.Height != image.Height)
            {
                int width = image.Width;
                int height = image.Height;
                PixelFormat pixelFormat = PixelFormat.Bgra8888;
                AlphaFormat alphaFormat = AlphaFormat.Premul;
                _reusableBitmap = new WriteableBitmap(
                    new PixelSize(width, height),
                    new Vector(96, 96),
                    pixelFormat,
                    alphaFormat);
            }

            using (var context = _reusableBitmap.Lock())
            {
                IntPtr ptr = context.Address;
                int stride = context.RowBytes;

                // Copy pixel data row by row
                for (int j = 0; j < image.Height; j++)
                {
                    image.Rows[j].CopyTo(row);
                    Marshal.Copy(_reusableRow, 0, ptr, image.Width * 4); // Assuming 24bpp RGB data
                    ptr += stride;
                }

                PreviewImage = _reusableBitmap;
            }
        }


        //Dispatcher.UIThread.Invoke(() =>
        // {

        // ProfilePictureUpdated?.Invoke();
        // });
    }

    public override void Dispose()
    {
        //base.Dispose();
        _shouldShowImage = false;
        if (_captureZone != null)
        {
            try
            {
                _screenCapture?.UnregisterCaptureZone(_captureZone);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
            
        _capturingService.FrameUpdated -= OnFrameUpdate;
        _capturingService.UnregisterUse();
    }
}