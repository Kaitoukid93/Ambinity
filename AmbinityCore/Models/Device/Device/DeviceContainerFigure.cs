using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Draw2D.Core.Shapes.Basic;

namespace AmbinityCore.Models.Device;

    public class DeviceContainerFigure : Rectangle
    {
        public DeviceContainerFigure(float x, float y, float width, float height) : base(x, y, width,
            height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            // SnapTargets = SnapTargets.Center | SnapTargets.MidPoints | SnapTargets.Vertices;
            PositionPropertyChanged += OnPositionChanged;
            SizePropertyChanged += OnSizeChanged;
        }

        private void OnSizeChanged(float newWidth, float newHeight)
        {
            _deviceVisualizer.UpdateContainerSize(newWidth, newHeight);
        }

        private void OnPositionChanged(float dx, float dy)
        {
            _deviceVisualizer.UpdateContainerOffset(dx, dy);
        }

        private DeviceVisualizer _deviceVisualizer;

        public void SetDevice(AmbinityDevice device)
        {
            _deviceVisualizer = new DeviceVisualizer(device);
            _deviceVisualizer.DeviceUpdate += OnDeviceUpdate;
            Width = (float)_deviceVisualizer.DeviceBounds.Width;
            Height = (float)_deviceVisualizer.DeviceBounds.Height;
            X = (float)_deviceVisualizer.DeviceBounds.X;
            Y = (float)_deviceVisualizer.DeviceBounds.Y;
        }

        private void OnDeviceUpdate()
        {
            Width = (float)_deviceVisualizer.DeviceBounds.Width;
            Height = (float)_deviceVisualizer.DeviceBounds.Height;
            X = (float)_deviceVisualizer.DeviceBounds.X;
            Y = (float)_deviceVisualizer.DeviceBounds.Y;
        }

        public override void Render(DrawingContext dc, double strokeThickness, Color strokeColor)
        {
            //get size and location from device property
            _deviceVisualizer.RenderDevice(dc, Canvas);
            var strokeBrush = new ImmutableSolidColorBrush(StrokeColor);
            var thickness = StrokeThickness;
            if (OverrideStrokeStyle)
            {
                strokeBrush = new ImmutableSolidColorBrush(strokeColor);
                thickness = (float)strokeThickness;
            }

            var screenPoint = Canvas.CoordinateSystem.ToScreenSpace(Position);
            var offset = new Point((float)screenPoint[0] - X, (float)screenPoint[1] - Y);

            // strokeBrush.Freeze();
            var pen = new Pen(strokeBrush, thickness, DashStyle);
            //  {
            //  DashStyle = DashStyle
            //  };
            // pen.Freeze();

            var fillBrush = new ImmutableSolidColorBrush(FillColor);
            // fillBrush.Freeze();

            Matrix translate = Matrix.CreateTranslation(offset.X, offset.Y);
            dc.PushTransform(translate);
            dc.DrawRectangle(fillBrush, pen,
                new Rect(new Point(X, Y), new Size(Width, Height)));

            // dc.Pop();
        }
    }
