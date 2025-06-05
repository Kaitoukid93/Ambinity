using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ambinity.Views.Draw2DCanvas;
using System.Windows.Input;
using Avalonia;
using Draw2D.Core.Shapes.Basic;
using Ambinity.Views.LayoutEditor.Canvas;

namespace Ambinity.Views.LayoutEditor.LEDLayoutCreator
{
    public partial class LEDLayoutCreatorViewModel : ObservableObject
    {
        // The main canvas viewmodel
        public LEDLayoutCreatorCanvasViewModel CanvasViewModel { get; }

        // The viewmodel for the floating panel
        [ObservableProperty]
        private LEDPropertiesViewModel? _selectedLEDProperties;

        public LEDLayoutCreatorViewModel(CanvasViewModelFactory canvasViewModelFactory)
        {
            CanvasViewModel = canvasViewModelFactory.Get<LEDLayoutCreatorCanvasViewModel>();
            //CanvasViewModel.Init(new Size(150, 150), enableSelection: true, enableRegionSelection: false);
            //CanvasViewModel.AddFigure(new Draw2D.Core.Shapes.Basic.Rectangle(0, 0, 25, 25),false);
            // Subscribe to selection changed event
            CanvasViewModel.SelectionChanged += OnCanvasSelectionChanged;
        }
        public void Init()
        {
            // Initialize the canvas with a specific size and properties
            CanvasViewModel.Init();
            // Optionally add a default figure to the canvas
            CanvasViewModel.AddFigure(new Draw2D.Core.Shapes.Basic.Rectangle(0, 0, 25, 25), false);
        }
        private void OnCanvasSelectionChanged()
        {
            // Get the selected item (assuming single selection)
            var selectedFigure = CanvasViewModel.Canvas?.Selection?.Primary;
            if (selectedFigure != null)
            {
                // Assume the figure has X, Y properties for position
                SelectedLEDProperties = new LEDPropertiesViewModel
                {
                    X = selectedFigure.X,
                    Y = selectedFigure.Y
                };
            }
            else
            {
                SelectedLEDProperties = null;
            }
        }
    }

    public partial class LEDPropertiesViewModel : ObservableObject
    {
        [ObservableProperty]
        private float x;

        [ObservableProperty]
        private float y;
    }
}
