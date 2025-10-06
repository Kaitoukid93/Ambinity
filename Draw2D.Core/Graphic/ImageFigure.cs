using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Draw2D.Core.Shapes.Basic;

namespace Draw2D.Core.Graphic;

public class ImageFigure : Rectangle
{
    private string _imagePath;
    public string ImagePath => _imagePath;
    private RenderTargetBitmap? _deviceImage;

    public ImageFigure(float x, float y, float width, float height) : base(x, y, width, height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
    public async Task SetImage(string imagePath)
    {
        _imagePath = imagePath;
        try
        {
            _deviceImage = await Task.Run(() => GetDeviceImage());
            Canvas.NeedsRepaint(this);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public override void Render(DrawingContext dc, double strokeThickness, Color strokeColor)
    {
        if (_deviceImage != null)
        {
            var strokeBrush = new ImmutableSolidColorBrush(StrokeColor);
            dc.DrawImage(_deviceImage, new Rect(_deviceImage.Size),
                new Rect(this.X, this.Y, this.Width, this.Height));
            var dash = new ImmutableDashStyle(new double[] { 7, 3 }, 1);
            var pen = new Pen(strokeBrush, StrokeThickness, dash);
            dc.DrawRectangle(null, pen,
                new Rect(new Point(X, Y), new Size(Width, Height)));
        }
    }

    private RenderTargetBitmap? GetDeviceImage()
    {

        // Create a bitmap that'll be used to render the device and LED images just once
        // Render 4 times the actual size of the device to make sure things look sharp when zoomed in
        RenderTargetBitmap renderTargetBitmap =
            new(new PixelSize((int)this.Width, (int)this.Height));
        try
        {
            using DrawingContext context = renderTargetBitmap.CreateDrawingContext();

            // Draw device background
            if (_imagePath != null && File.Exists(_imagePath))
            {
                using Bitmap bitmap = new(_imagePath);
                using Bitmap scaledBitmap = bitmap.CreateScaledBitmap(renderTargetBitmap.PixelSize);
                context.DrawImage(scaledBitmap, new Rect(scaledBitmap.Size));
            }
            return renderTargetBitmap;
        }
        catch (Exception ex)
        {
            // Log or handle rendering exception
            return null;
        }
    }
}
