using System.Globalization;
using System.Runtime.InteropServices;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Geography;
using AmbinityCore.Repositories;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Draw2D.Core;
using Draw2D.Core.Graphic;
using Draw2D.Core.Shapes.Basic;
using Draw2D.Core.Utlils;

namespace AmbinityCore.Models.Lighting.Zone;

public class LightingZoneFigure : ContainerFigure, IAssetSelectable
{
    private FormattedText _text;
    private Point _textOrigin = new Point(5, -30);
    private Pen _defaultPen = new Pen(new SolidColorBrush(Avalonia.Media.Colors.Chartreuse));
    private WriteableBitmap _zoneReusableBitmap;
    private bool _isValid;
    private Rect _canvasRect;

    public LightingZoneFigure(float x, float y, float width, float height) : base(x, y, width, height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public override void SetChild(IPositionAware zone)
    {
        ChildItem = zone;
        ItemVisualizer = new LightingZoneVisualizer(ChildItem);
        ItemVisualizer.ItemUpdated += ZoneUpdated;
        _text = new FormattedText((zone as LightingZone).Name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            Typeface.Default,
            20, new ImmutableSolidColorBrush(Avalonia.Media.Colors.Gray));
        _text.MaxTextWidth = 200;
        _text.MaxLineCount = 1;
        _text.Trimming = TextTrimming.CharacterEllipsis;
    }

    private void OnFrameUpdated()
    {
        Canvas.NeedsRepaint(this);
    }


    /// <summary>
    /// when zone has internal update, notify the container
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void ZoneUpdated()
    {
        X = ChildItem.X;
        Y = ChildItem.Y;
        Width = ChildItem.Width;
        Height = ChildItem.Height;
        UpdateBitmap();
        Canvas.NeedsRepaint(this);
        Canvas.NeedsRepaint(this);
    }

    private void UpdateBitmap()
    {
        Vector dpi = new Vector(96, 96);
        _zoneReusableBitmap = new WriteableBitmap(
            new PixelSize((int)ChildItem.Width, (int)ChildItem.Height),
            dpi,
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);
    }

    public override void Render(DrawingContext dc, double strokeThickness, Color strokeColor)
    {
        //get size and location from device property
        //check zone collision
        var rect = new Rect(X, Y, Width, Height);
        int count = 0;
        foreach (var figure in Canvas.Figures.Where(f => f is LightingZoneFigure && f != this))
        {
            var rect2 = new Rect(figure.X, figure.Y, figure.Width, figure.Height);
            if (rect.Intersects(rect2))
                count++;
        }

        _canvasRect = new Rect(0, 0, Canvas.Width, Canvas.Height);
        _isValid = count <= 0 && _canvasRect.Contains(rect);
        ItemVisualizer.Render(dc, Canvas);
        // using (var frameBuffer = _zoneReusableBitmap.Lock())
        // {
        //     lock (_zone.Buffer.FrameLock)
        //     {
        //         Marshal.Copy(_zone.Buffer.PixelData, 0, frameBuffer.Address, _zone.Buffer.PixelData.Length);
        //     }
        //     dc.DrawImage(_zoneReusableBitmap,new Rect(X,Y,_zone.Width,_zone.Height));
        // }


        var strokeBrush = new ImmutableSolidColorBrush(ChildItem.GetDisplayColor()??strokeColor);
        var thickness = StrokeThickness;
        if (OverrideStrokeStyle)
        {
            strokeBrush = new ImmutableSolidColorBrush(ChildItem.GetDisplayColor()??strokeColor);
            thickness = (float)strokeThickness;
        }
        
        var screenPoint = Canvas.CoordinateSystem.ToScreenSpace(Position);
        var offset = new Point((float)screenPoint[0] - X, (float)screenPoint[1] - Y);
        
        var fillBrush = new ImmutableSolidColorBrush(FillColor);
        if (!_isValid)
            strokeBrush = new ImmutableSolidColorBrush(Avalonia.Media.Colors.Red);
        if (IsMouseOver && !IsSelected)
        {
            fillBrush = new ImmutableSolidColorBrush(Avalonia.Media.Colors.Gray.AdjustOpacity(0.2));
            strokeBrush = new ImmutableSolidColorBrush(Avalonia.Media.Colors.Orange);
        }
        
        var pen = new Pen(strokeBrush, thickness, DashStyle);
        var immutablePen = pen.ToImmutable();
        
        Matrix translate = Matrix.CreateTranslation(offset.X, offset.Y);
        dc.PushTransform(translate);
        var zone = ChildItem as LightingZone;
        
            if (zone.Shape == ZoneShapeEnum.Ellipse)
            {
                dc.DrawEllipse(fillBrush, pen,
                    new Rect(new Point(X, Y), new Size(Width, Height)));
            }
            else if (zone.Shape == ZoneShapeEnum.Rectangle)
            {
                dc.DrawRectangle(fillBrush, pen,
                    new Rect(new Point(X, Y), new Size(Width, Height)));
            }
            else if (zone.Shape == ZoneShapeEnum.Polyline)
            {
                if(zone.Points.Count ==0)
                    return;
                var geom = new StreamGeometry();
                using (StreamGeometryContext ctx = geom.Open())
                {
                    var StartPoint = new Draw2D.Core.Geo.Point((float)zone.Points[0].X, (float)zone.Points[0].Y);
                    var startVertex = Canvas.CoordinateSystem.ToScreenSpace(StartPoint);
                    ctx.BeginFigure(new Avalonia.Point(startVertex[0], startVertex[1]), false);
                    int pointCount = 0;
                    foreach (var point in zone.Points)
                    {
                        var GeoPoint = new Draw2D.Core.Geo.Point((float)point.X, (float)point.Y);
                        if (pointCount == 0)
                        {
                            pointCount++;
                            continue;
                        }
                        
                        var vertex = Canvas.CoordinateSystem.ToScreenSpace(GeoPoint);
                        var v = new Avalonia.Point(vertex[0], vertex[1]);
                        ctx.LineTo(v);
                        pointCount++;
                    }
                }
                dc.DrawGeometry(null, pen, geom);
            }
        //  var currentZoomValue = 2 / strokeThickness;
        //  double adaptiveFontSize = 12d / currentZoomValue;
        //  _text.SetFontSize(adaptiveFontSize);
        //  if (IsSelected || IsMouseOver)
        //    _text.SetForegroundBrush(strokeBrush);
        // else
        //  {
        //     _text.SetForegroundBrush(new ImmutableSolidColorBrush(Avalonia.Media.Colors.Gray));
        //  }

        //  if (currentZoomValue is > 0.4 and < 5)
        //   dc.DrawText(_text, new Point(X, Y - 40));
        // dc.Pop();
    }

    #region IAssetSelectable implementation

    public ICollectableItem AssetSelectableProperty => ChildItem as LightingZone;

    #endregion
}