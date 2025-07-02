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

namespace Ambinity.Views.LayoutEditor.LEDLayoutCreator
{
    public partial class LEDLayoutCreatorViewModel : ObservableObject
    {
        private DialogService _dialogService;
        private IWindowService _windowService;
        private CanvasViewModelFactory _canvasViewModelFactory;
        private int _ledCount;

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


        public LEDLayoutCreatorViewModel(CanvasViewModelFactory canvasViewModelFactory, IWindowService windowService)
        {

            _windowService = windowService;
            _canvasViewModelFactory = canvasViewModelFactory;
            //CanvasViewModel.Init(new Size(150, 150), enableSelection: true, enableRegionSelection: false);
            //CanvasViewModel.AddFigure(new Draw2D.Core.Shapes.Basic.Rectangle(0, 0, 25, 25),false);
            // Subscribe to selection changed event


        }
        public void Init(int width, int height, int ledCount, string imagePath)
        {
            // Initialize the canvas with a specific size and properties
            var vm = _canvasViewModelFactory.Get<LEDLayoutCreatorCanvasViewModel>();
            vm.Init(1000, 1000);
            vm.SetDeviceImage(imagePath, width, height);
            //add rectangle to canvas based on ledcount
            _ledCount = ledCount;
            CanvasViewModel = vm;
            //PopulateLEDs();
            LEDPropertiesViewModel = new LEDPropertiesViewModel();

        }
        /// <summary>
        /// populate led based on led count
        /// </summary>
        private void PopulateLEDs()
        {
            const double canvasWidth = 500;
            const double canvasHeight = 500;
            const double maxWidth = 20;
            const double maxHeight = 20;

            // Calculate how many columns and rows can fit
            int columns = (int)(canvasWidth / maxWidth);
            int rows = (int)(canvasHeight / maxHeight);

            // Calculate actual rectangle size to fit all LEDs if possible
            double rectWidth = Math.Min(canvasWidth / columns, maxWidth);
            double rectHeight = Math.Min(canvasHeight / rows, maxHeight);
            int count = 0;

            for (int row = 0; row < rows && count < _ledCount; row++)
            {
                for (int col = 0; col < columns && count < _ledCount; col++)
                {
                    double x = col * rectWidth;
                    double y = row * rectHeight;
                    var geometryString = "M0,0 H20 V20 H0 Z";
                    var led = new AmbinityLED(new ArgbLed(), null, (float)x, (float)y, (float)rectWidth, (float)rectHeight, count, false, geometryString);
                    led.X = (float)x;
                    led.Y = (float)y;
                    var containerFigure = led.GetContainer();
                    containerFigure.SetChild(led);
                    containerFigure.MinHeight = 5;
                    containerFigure.MinWidth = 5;
                    CanvasViewModel.AddFigure(containerFigure, false);
                    count++;
                }
            }
        }

        // private void OnLayoutSizeChanged(float width, float height)
        // {

        //     var vm = _canvasViewModelFactory.Get<LEDLayoutCreatorCanvasViewModel>();
        //     vm.Init((int)width, (int)height);
        //     CanvasViewModel = vm;
        //     CanvasViewModel.SetDeviceImage(CurrentImagePath);
        // }

        // private void OnDeviceImageChanged(string imagePath)
        // {
        //     CurrentImagePath = imagePath;
        //     CanvasViewModel.SetDeviceImage(CurrentImagePath);
        // }

        // public ICommand AddLEDCommand { get; set; }



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
            var selectedFigure = CanvasViewModel.Canvas?.Selection?.Primary;
            if (selectedFigure != null)
            {
                // Assume the figure has X, Y properties for position
                LEDPropertiesViewModel = new LEDPropertiesViewModel
                {
                    X = selectedFigure.X,
                    Y = selectedFigure.Y
                };
            }
            else
            {
                LEDPropertiesViewModel = null;
            }
        }
    }

    public class LEDPropertiesViewModel : ObservableObject
    {

        private float _x;
        private float _y;
        public float X
        {
            get => _x;
            set
            {
                _x = value;
                OnPropertyChanged();
            }
        }
        public float Y
        {
            get => _y;
            set
            {
                _y = value;
                OnPropertyChanged();
            }
        }
    }



}
