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

namespace Ambinity.Views.LayoutEditor.LEDLayoutCreator
{
    public partial class LEDLayoutCreatorViewModel : ObservableObject
    {
        private DialogService _dialogService;
        private IWindowService _windowService;
        private CanvasViewModelFactory _canvasViewModelFactory;

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
        public void Init(int width, int height,int ledCount, string imagePath)
        {
            // Initialize the canvas with a specific size and properties
            var vm = _canvasViewModelFactory.Get<LEDLayoutCreatorCanvasViewModel>();
            vm.Init(width,height);
            vm.SetDeviceImage(imagePath);
            //add rectangle to canvas based on ledcount
            CanvasViewModel = vm;
            LEDPropertiesViewModel = new LEDPropertiesViewModel();

        }

        private void OnLayoutSizeChanged(float width, float height)
        {

            var vm = _canvasViewModelFactory.Get<LEDLayoutCreatorCanvasViewModel>();
            vm.Init((int)width, (int)height);
            CanvasViewModel = vm;
            CanvasViewModel.SetDeviceImage(CurrentImagePath);
        }

        private void OnDeviceImageChanged(string imagePath)
        {
            CurrentImagePath = imagePath;
            CanvasViewModel.SetDeviceImage(CurrentImagePath);
        }

        public ICommand AddLEDCommand { get; set; }



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

    public class LayoutPropertiesViewModel : ViewModelBase
    {
        public LayoutPropertiesViewModel()
        {
            _windowService = Ioc.Default.GetRequiredService<IWindowService>();
            SelectDeviceImageCommand = new AsyncRelayCommand(SelectDeviceImage);
        }

        private IWindowService _windowService;

        public ICommand SelectDeviceImageCommand { get; set; }

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
            }
        }

        private async Task SelectDeviceImage()
        {
            string[]? result = await _windowService.CreateOpenFileDialog()
                 .WithTitle("Select Device Image")
                 .HavingFilter(f => f.WithExtension("png").WithName("png file"))
                 .WithDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures))
                 .ShowAsync();
            if (result == null || result.Length == 0)
                return;
            ImagePath = result[0];

        }
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

    public class LayoutEditorToolsViewModel : ViewModelBase
    {
        public LayoutEditorToolsViewModel()
        {
            OpenAddLEDDialogCommand = new AsyncRelayCommand(OpenAddLEDDialog);
            ImportLEDGeometryCommand = new AsyncRelayCommand(OpenLEDGeometryBrowser);
            ClearCanvasCommand = new RelayCommand(ClearCanvas);
            FitCanvasViewCommand = new RelayCommand(FitCanvas);
        }
        public ICommand OpenAddLEDDialogCommand { get; set; }
        public ICommand ImportLEDGeometryCommand { get; set; }
        public ICommand ClearCanvasCommand { get; set; }
        public ICommand FitCanvasViewCommand { get; set; }
        private async Task OpenAddLEDDialog()
        {

        }
        private async Task OpenLEDGeometryBrowser()
        {

        }
        private void ClearCanvas()
        {

        }
        private void FitCanvas()
        {

        }
    }

}
