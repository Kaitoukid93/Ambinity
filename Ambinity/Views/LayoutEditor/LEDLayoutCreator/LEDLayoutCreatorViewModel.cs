using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ambinity.Views.Draw2DCanvas;
using System.Windows.Input;
using Avalonia;
using Draw2D.Core.Shapes.Basic;
using Ambinity.Views.LayoutEditor.Canvas;
using System.Threading.Tasks;
using Ambinity.Windows;
using Ambinity.Services;
using Ambinity.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;
using SkiaSharp;
using System.IO;
using Avalonia.Controls.Shapes;
using AmbinityCore.Models.Device.LED;
using Avalonia.Media;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using ReactiveUI;
using System.Linq;
using Draw2D.Core;
using Draw2D.Core.Graphic;
using AmbinityCore;
using Path = System.IO.Path;
using AmbinityCore.Models.Device;
using Serilog;
using adrilight_shared.Models.Device.SlaveDevice;
using System.Collections.ObjectModel;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.Device.Zone.Spot;
using AmbinityCore.Utils;
using Vortice.Mathematics;
using AmbinityCore.Helpers;
using AmbinityCore.Repositories;

namespace Ambinity.Views.LayoutEditor.LEDLayoutCreator
{
    public partial class LEDLayoutCreatorViewModel : ViewModelBase
    {
        private DialogService _dialogService;
        private AmbinityDeviceLayoutRepository _repository;
        private IWindowService _windowService;
        private CanvasViewModelFactory _canvasViewModelFactory;
        private int _ledCount;
        private float _scale;

        //user desired layout properties
        private float _layoutWidth;
        private float _layoutHeight;

        // The main canvas viewmodel
        private LEDLayoutCreatorCanvasViewModel _canvasViewModel;
        public LEDLayoutCreatorCanvasViewModel CanvasViewModel
        {
            get => _canvasViewModel;
            set
            {
                _canvasViewModel = value;
                OnPropertyChanged();
            }
        }


        public LEDLayoutCreatorViewModel(CanvasViewModelFactory canvasViewModelFactory, IWindowService windowService, AmbinityDeviceLayoutRepository repository)
        {
            _repository = repository;
            _windowService = windowService;
            _canvasViewModelFactory = canvasViewModelFactory;
            LEDPropertiesViewModel = new LEDPropertiesViewModel();
            //CanvasViewModel.Init(new Size(150, 150), enableSelection: true, enableRegionSelection: false);
            //CanvasViewModel.AddFigure(new Draw2D.Core.Shapes.Basic.Rectangle(0, 0, 25, 25),false);
            // Subscribe to selection changed event


        }
        public bool Init(int width, int height, List<AmbinityLEDLayout> leds, string? imagePath = null)
        {
            // Initialize the canvas with a specific size and properties
            if (width <= 0 || height <= 0)
            {
                return false;
            }
            // if (width > 1000 || height > 1000)
            // {
            //     return false;
            // }
            _layoutWidth = width;
            _layoutHeight = height;
            var vm = _canvasViewModelFactory.Get<LEDLayoutCreatorCanvasViewModel>();
            float adaptedWidth = 500;
            _scale = adaptedWidth / width;
            float adaptedHeight = (float)height * _scale;
            float offsetX = (1000 - adaptedWidth) / 2;
            float offsetY = (1000 - adaptedHeight) / 2;
            vm.Init(1000, 1000);
            vm.SetDeviceImage(imagePath, offsetX, offsetY, adaptedWidth, adaptedHeight);
            //add rectangle to canvas based on ledcount
            _ledCount = leds == null ? 0 : leds.Count;
            CanvasViewModel = vm;
            CanvasViewModel.SelectionChanged += OnCanvasSelectionChanged;
            CanvasViewModel.OnLayoutSaveRequest += SaveLayout;
            PopulateLEDs(leds, offsetX, offsetY, _scale);
            return true;


        }

        private void SaveLayout(List<Figure> leds, ImageFigure image, string layoutName, string layoutDescription)
        {
            //cache is no longer needed
            
            // var cachePath = Path.Combine(Constants.CacheFolderPath, "LayoutCreator");
            // if (!Directory.Exists(cachePath))
            //     return;

            if (leds == null)
            {
                Log.Error("Can not export this Layout because there is no LED found");
                return;
            }
            var dev = new ARGBLEDSlaveDevice();
            dev.Name = layoutName;
            dev.Description = layoutDescription;
            var zone = new ObservableCollection<LEDSetup>();
            var ledSetup = new LEDSetup();
            var boundingBox = image.BoundingBox;
            var ledsBoundingBox = GeometryUltilities.GetBoundingBox(leds);
            foreach (var figure in leds)
            {
                if (figure is LEDContainerFigure ledContainerFigure)
                {
                    var led = ledContainerFigure.ChildItem as AmbinityLED;
                    if (led != null)
                    {
                        var spot = new DeviceSpot();
                        spot.Index = led.Index ?? 0;
                        spot.Top = (led.Y - ledsBoundingBox.Y) / _scale;
                        spot.Left = (led.X - ledsBoundingBox.X) / _scale;
                        spot.Width = led.Width / _scale;
                        spot.Height = led.Height / _scale;
                        spot.Geometry = led.Geometry;
                        ledSetup.Spots.Add(spot);
                    }
                }
            }
            ledSetup.Top = (ledsBoundingBox.Y - boundingBox.Y) / _scale;
            ledSetup.Left = (ledsBoundingBox.X - boundingBox.X) / _scale;

            zone.Add(ledSetup);
            dev.ControlableZones = zone;

            //rename image
            var imagePath = image.ImagePath;
            dev.Image = new ImageVisual() { Width = _layoutWidth, Height = _layoutHeight };
            _repository.CreateLayout(layoutName, layoutDescription, dev, imagePath, imagePath);
        }


        /// <summary>
        /// populate led based on led count, bring leds to center and scale to 500 in width
        /// </summary>
        private void PopulateLEDs(List<AmbinityLEDLayout> leds, float offsetX = 250, float offsetY = 250, float scale = 1f)
        {

            foreach (var led in leds)
            {
                var missingLED = new AmbinityLED(new ArgbLed(), null,
                   led.X,
                   led.Y,
                   led.Width,
                   led.Height,
                   led.Index,
                   true,
                   led.Geometry);
                missingLED.X = led.X * scale + offsetX;
                missingLED.Y = led.Y * scale + offsetY;
                missingLED.Width = missingLED.Width * scale;
                missingLED.Height = missingLED.Height * scale;
                var containerFigure = missingLED.GetContainer();
                containerFigure.SetChild(missingLED);
                containerFigure.MinHeight = 5;
                containerFigure.MinWidth = 5;
                CanvasViewModel.AddFigure(containerFigure, false);
            }
        }

        public LEDPropertiesViewModel? LEDPropertiesViewModel { get; set; }
        private string _currentImagePath = string.Empty;
        public string CurrentImagePath
        {
            get => _currentImagePath;
            set
            {
                _currentImagePath = value;
                OnPropertyChanged(nameof(CurrentImagePath));
            }
        }


        private void OnCanvasSelectionChanged()
        {
            // Get the selected item (assuming single selection)
            var selectedFigures = CanvasViewModel?.Canvas?.Selection?.All;
            if (selectedFigures != null)
            {
                LEDPropertiesViewModel?.Init(selectedFigures);

            }
            else
            {
                //LEDPropertiesViewModel = null;
                LEDPropertiesViewModel?.Init(null);
            }
        }
        public override void Dispose()
        {
            // Unsubscribe from events if necessary
            CanvasViewModel?.Dispose();
            CanvasViewModel = null;
        }
    }





}
