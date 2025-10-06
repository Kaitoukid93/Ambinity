
using System;
using System.Collections.Generic;
using Ambinity.Views.LayoutEditor.Canvas;
using AmbinityCore.Models.Device.LED;
using Draw2D.Core.Geo;
using Draw2D.Core.Handles;
using Draw2D.Core.Layout.Connection;
using Draw2D.Core.Shapes.Basic;
using QuickGraph;
using Rectangle = Draw2D.Core.Shapes.Basic.Rectangle;

namespace Ambinity.Views.LayoutEditor.LEDLayoutCreator.Tools
{
    public class LEDTool : ToolBase
    {
        private LEDContainerFigure _led;
        private bool _isInitialized;
        private Rectangle _handle;
        public LEDLayoutCreatorCanvasViewModel CanvasViewModel { get; set; }
        private string _geometry = "M0,0 H20 V20 H0 Z";
        private int _toolSize = 20;
        private int minSize = 5;
        private int maxSize = 200;
        private int sizeStep = 10;
        public LEDTool(string geometry)
        {
            _geometry = geometry;
        }
        public void IncreaseToolSize()
        {
            if (_toolSize + sizeStep <= maxSize)
            {
                var centerX = _handle.X + _handle.Width / 2;
                var centerY = _handle.Y + _handle.Height / 2;
                _toolSize += sizeStep;
                _handle.ForceSetDimensions(new Draw2D.Core.Geo.Rectangle(_handle.X, _handle.Y, _toolSize, _toolSize));
                _handle.ForceSetPositionCenter(centerX, centerY);
            }
        }
        public void DecreaseToolSize()
        {
            if (_toolSize - sizeStep >= minSize)
            {
                var centerX = _handle.X + _handle.Width / 2;
                var centerY = _handle.Y + _handle.Height / 2;
                _toolSize -= sizeStep;
                _handle.ForceSetDimensions(new Draw2D.Core.Geo.Rectangle(_handle.X, _handle.Y, _toolSize, _toolSize));
                _handle.ForceSetPositionCenter(centerX, centerY);
            }
        }

        public override void OnMouseLeftDown(Draw2D.Core.Canvas canvas, float mouseX, float mouseY, bool isShiftKey, bool isCtrlKey)
        {

            var led = new AmbinityLED(new ArgbLed(), null, 100, 100, _toolSize, _toolSize, 0, false, _geometry);
            var fig = new LEDContainerFigure(100, 100, _toolSize, _toolSize);
            led.X = mouseX - fig.Width / 2;
            led.Y = mouseY - fig.Height / 2;
            fig.SetChild(led);
            fig.MinWidth = 5;
            fig.MinHeight = 5;
            fig.IsResizable = true;
            if (CanvasViewModel != null)
            {
                CanvasViewModel.AddFigure(fig, true);
            }

        }
        public override void OnMouseMove(Draw2D.Core.Canvas canvas, float mouseX, float mouseY, bool isShiftKey, bool isCtrlKey)
        {
            if (!_isInitialized)
            {

                _handle = new CustomGeometryShape(_geometry, mouseX, mouseY, _toolSize, _toolSize);

                canvas.Selection.Clear();

                canvas.AddFigure(_handle);
                _isInitialized = true;
            }
            _handle.ForceSetPositionCenter(mouseX, mouseY);

        }

        private void ExecuteOnDone(Draw2D.Core.Canvas canvas)
        {

            // _led.Select();
            foreach (var snapPolicy in canvas.GetSnapPolicies())
            {
                snapPolicy.EndSnapping(canvas);
            }
            canvas.RemoveFigure(_handle);
            OnDone(this);
        }

        public override void OnMouseRightDown(Draw2D.Core.Canvas canvas, float mouseX, float mouseY, bool isShiftKey, bool isCtrlKey)
        {

            Cancel(canvas);
            canvas.UnInstallCurrentTool();
        }


        public override void Cancel(Draw2D.Core.Canvas canvas)
        {
            base.Cancel(canvas);

            foreach (var snapPolicy in canvas.GetSnapPolicies())
            {
                snapPolicy.EndSnapping(canvas);
            }

            canvas.RemoveFigure(_led);
            canvas.RemoveFigure(_handle);
        }

        public override void Reroute(IReadOnlyList<Point> points)
        {
            throw new NotImplementedException();
        }

        public override void OnMouseLeftDoubleClick(Draw2D.Core.Canvas canvas, float mouseX, float mouseY, bool isShiftKey, bool isCtrlKey)
        {
            ExecuteOnDone(canvas);
        }

    }
}
