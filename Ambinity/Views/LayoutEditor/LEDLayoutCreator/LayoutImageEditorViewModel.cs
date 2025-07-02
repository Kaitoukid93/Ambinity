using System;
using System.IO;
using System.Windows.Input;
using Ambinity.ViewModels;
using Ambinity.Views.LayoutEditor.Canvas;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Draw2D.Core.Policies.FigurePolicy;
using Draw2D.Core.Shapes.Basic;
using SkiaSharp;

namespace Ambinity.Views.LayoutEditor.LEDLayoutCreator;

public class LayoutImageEditorViewModel : ViewModelBase
{
    public event Action OnUserAccept;
    public LayoutImageEditorViewModel(CanvasViewModelFactory canvasViewModelFactory)
    {
        _canvasViewModelFactory = canvasViewModelFactory;
        AcceptUserEditCommand = new RelayCommand(CropImage);

    }
    private CropRectangleFigure _cropFig;
    private string _outputPath;
    private string _imagePath;

    /// <summary>
    /// because this is one time usage so i'm too lazy to implement an ImageEditingService lul...
    /// </summary>
    public void CropImage()
    {
        try
        {
            if (string.IsNullOrEmpty(_imagePath) || !File.Exists(_imagePath))
                throw new FileNotFoundException("Image file not found.", _imagePath);

            // Load the image from file
            using (var input = File.OpenRead(_imagePath))
            using (var original = SKBitmap.Decode(input))
            {
                if (original == null)
                    throw new Exception("Failed to decode the image. The file may be corrupted or in an unsupported format.");

                // Validate crop rectangle
                var cropRect = new SKRectI((int)_cropFig.X,
                 (int)_cropFig.Y,
                  (int)_cropFig.X + (int)_cropFig.Width,
                   (int)_cropFig.Y + (int)_cropFig.Height);
                if (cropRect.Left < 0 || cropRect.Top < 0 ||
                    cropRect.Right > original.Width || cropRect.Bottom > original.Height ||
                    cropRect.Width <= 0 || cropRect.Height <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(_cropFig), "Crop rectangle is out of image bounds or invalid.");
                }

                // Crop the image
                using (var cropped = new SKBitmap(cropRect.Width, cropRect.Height))
                {
                    if (!original.ExtractSubset(cropped, cropRect))
                        throw new Exception("Failed to extract the cropped region from the image.");

                    // Overwrite the original image with the cropped PNG
                    using (var output = File.Open(_outputPath, FileMode.Create, FileAccess.Write))
                    using (var image = SKImage.FromBitmap(cropped))
                    using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                    {
                        if (data == null)
                            throw new Exception("Failed to encode the cropped image as PNG.");

                        data.SaveTo(output);
                    }
                }
                OnUserAccept?.Invoke();
            }
        }
        catch (Exception ex)
        {
            // Handle or log the error as needed
            Console.Error.WriteLine($"Error cropping image: {ex.Message}");
            // Optionally, rethrow or handle differently
            // throw;
        }
    }

    public ICommand AcceptUserEditCommand { get; }
    private CanvasViewModelFactory _canvasViewModelFactory;

    // The main canvas viewmodel
    private LayoutImageEditorCanvasViewModel _canvasViewModel;
    public LayoutImageEditorCanvasViewModel CanvasViewModel
    {
        get => _canvasViewModel;
        set
        {
            _canvasViewModel = value;
            OnPropertyChanged();
        }
    }
    public void Init(int width, int height, string imagePath,string output)
    {
        // Initialize the canvas with a specific size and properties
        _outputPath = output;
        _imagePath = imagePath;
        var vm = _canvasViewModelFactory.Get<LayoutImageEditorCanvasViewModel>();
        //add scale for better drawing
        vm.Init(width, height);
        vm.SetDeviceImage(imagePath);
        //add rectangle to canvas based on ledcount
        CanvasViewModel = vm;
        _cropFig = new CropRectangleFigure(0, 0, width, height);
        var visiblePolicyRect = new Rectangle(0, 0, width, height);
        visiblePolicyRect.FillColor = Colors.Transparent;
        visiblePolicyRect.IsDragable = false;
        visiblePolicyRect.IsSelectable = false;
        var regionPolicy = new FigureRegionDragDropEditPolicy(visiblePolicyRect);
        _cropFig.InstallEditPolicy(regionPolicy);
        CanvasViewModel.AddFigure(_cropFig, false);
        _cropFig.Select();

    }
}
