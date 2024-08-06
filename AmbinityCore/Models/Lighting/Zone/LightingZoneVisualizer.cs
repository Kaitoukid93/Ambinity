using System.Runtime.InteropServices;
using AmbinityCore.Models.Geography;
using AmbinityCore.Visualizer;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Draw2D.Core;
using Draw2D.Core.Graphic;

namespace AmbinityCore.Models.Lighting.Zone;

public class LightingZoneVisualizer : ICanvasVisualizerItem
{
    public event Action ItemUpdated;
    private Rect _zoneBounds;
    private Pen _defaultPen = new Pen(new SolidColorBrush(Avalonia.Media.Colors.Chartreuse));
    public Rect Bounds => _zoneBounds;
    private RenderTargetBitmap? _zoneImage;
    public IPositionAware Item => _zone;
    private LightingZone _zone;
    private bool _loading;


    public LightingZoneVisualizer(IPositionAware zone)
    {
        _zone = zone as LightingZone;
        _zone.LocationUpdated += OnZoneLocationChanged;
        _zone.SizeUpdated += OnZoneSizeChanged;
        SetupZone();
    }

  

    private void OnZoneSizeChanged()
    {
        SetupZone();
        ItemUpdated?.Invoke();
    }

    private void OnZoneLocationChanged()
    {
        SetupZone();
        ItemUpdated?.Invoke();
    }
    public void UpdateContainerOffset(float dx, float dy)
    {
        _zone.X += dx;
        _zone.Y += dy;
        _zoneBounds = MeasureZone();
    }

    public void UpdateContainerSize(float x, float y)
    {
        _zone.Width = x;
        _zone.Height = y;
        _zoneBounds = MeasureZone();
    }

    public void Render(DrawingContext dc, Canvas canvas)
    {
        if (_zone == null || _zoneBounds.Width == 0 || _zoneBounds.Height == 0 || _loading)
            return;
        // Determine the scale required to fit the desired size of the control
        //double scale = Math.Min(Bounds.Width / _deviceBounds.Width, Bounds.Height / _deviceBounds.Height);
        DrawingContext.PushedState? boundsPush = null;
        try
        {
            // Scale the visualization in the desired bounding box
            //if (Bounds.Width > 0 && Bounds.Height > 0)
            boundsPush = dc.PushTransform(Matrix.CreateScale(_zone.Scale, _zone.Scale));

            // Apply device rotation
            using DrawingContext.PushedState translationPush =
                dc.PushTransform(Matrix.CreateTranslation(_zone.X / _zone.Scale, _zone.Y / _zone.Scale));
            using DrawingContext.PushedState rotationPush =
                dc.PushTransform(Matrix.CreateRotation(Matrix.ToRadians(_zone.Rotation)));
            //render zone bitmap and info
          
            
            
            // todo: render zone info 
        }
        finally
        {
            boundsPush?.Dispose();
        }
    }

    private async Task SetupZone()
    {
        if (_zone == null)
            return;
        _zoneBounds = MeasureZone();
        _loading = true;
        _loading = false;
    }

    

    private Rect MeasureZone()
    {
        if (_zone == null || float.IsNaN(_zone.Width) || float.IsNaN(_zone.Height))
            return new Rect();

        Rect zoneRect = new(0, 0, _zone.Width, _zone.Height);
        Geometry geometry = new RectangleGeometry(zoneRect);
        geometry.Transform = new TransformGroup()
        {
            Children =
            {
                new RotateTransform(_zone.Rotation),
                new ScaleTransform(_zone.Scale, _zone.Scale),
                new TranslateTransform(_zone.X, _zone.Y)
            }
        };
        
        return geometry.Bounds;
    }
}