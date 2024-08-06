using AmbinityCore.CapturingService;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using AmbinityCore.Models.Profile;
using Avalonia;
using Avalonia.Media;
using Draw2D.Core.Graphic;

namespace AmbinityCore.LightingEngines;

public class ColorPaletteEngine : IColorEngine
{
    private double _startIndex = 0;
    private Color[] _colorBank;
    private LightingZone _zone;
    private FrameBuffer _buffer;
    private ColorPaletteConfiguration config;
    private Rect zoneRect;
    private Rect zoneAbsoluteRect;
    private List<Point[]> _pixelPath;

    public ColorPaletteEngine(FrameBuffer buffer)
    {
        CaptureType = CapturingType.None;
        _buffer = buffer;
    }

    public void Render()
    {
        int left = (int)_zone.X;
        int top = (int)_zone.Y;
        int width = (int)_zone.Width;
        int height = (int)_zone.Height;
        _startIndex += 5;
        if (_startIndex > _colorBank.Length)
        {
            _startIndex = 0;
        }

        int position = 0;
        int offSet = 0;
        for (int l = 0; l < _pixelPath.Count; l++)
        {
            //problem with offset??
            var line = _pixelPath[l];
            for (int i = 0; i < line.Length; i++)
            {
                position = Convert.ToInt32(Math.Floor(_startIndex + i + offSet));
                position %= _colorBank.Length;
                //also do the stroke thickness
                //x center expand 5 to left
                var startPoint = line.First();
                var endPoint = line.Last();
                if (endPoint.X > startPoint.X && endPoint.Y < startPoint.Y)
                {
                    for (int q = 0; q < 100; q++)
                    {
                        var pX = (int)line[i].X + q;
                        var pY = (int)line[i].Y;
                        if (pX >= _zone.Width + _zone.X || pY >= _zone.Height + _zone.Y)
                        {
                            continue;
                        }

                        ColorComputing.PlotPixel(_buffer, pX, pY, _colorBank[position].R,
                            _colorBank[position].G,
                            _colorBank[position].B);
                    }

                    for (int p = 0; p < 100; p++)
                    {
                        var pX = (int)line[i].X;
                        var pY = (int)line[i].Y + p;
                        if (pX >= _zone.Width + _zone.X || pY >= _zone.Height + _zone.Y)
                        {
                            continue;
                        }

                        ColorComputing.PlotPixel(_buffer, pX, pY, _colorBank[position].R,
                            _colorBank[position].G,
                            _colorBank[position].B);
                    }
                }
                else if (endPoint.X < startPoint.X && endPoint.Y > startPoint.Y)
                {
                    for (int q = 0; q < 100; q++)
                    {
                        var pX = (int)line[i].X - q;
                        var pY = (int)line[i].Y;
                        if (pX <= _zone.X || pY <= _zone.Y)
                        {
                            continue;
                        }

                        ColorComputing.PlotPixel(_buffer, pX, pY, _colorBank[position].R,
                            _colorBank[position].G,
                            _colorBank[position].B);
                    }

                    for (int p = 0; p < 100; p++)
                    {
                        var pX = (int)line[i].X;
                        var pY = (int)line[i].Y - p;
                        if (pX <= _zone.X || pY <= _zone.Y)
                        {
                            continue;
                        }

                        ColorComputing.PlotPixel(_buffer, pX, pY, _colorBank[position].R,
                            _colorBank[position].G,
                            _colorBank[position].B);
                    }
                }
                else if (endPoint.X > startPoint.X && endPoint.Y > startPoint.Y)
                {
                    for (int p = 0; p < 100; p++)
                    {
                        var pX = (int)line[i].X + p;
                        var pY = (int)line[i].Y;
                        if (pX >= _zone.Width + _zone.X || pY >= _zone.Height + _zone.Y)
                        {
                            continue;
                        }

                        ColorComputing.PlotPixel(_buffer, pX, pY, _colorBank[position].R,
                            _colorBank[position].G,
                            _colorBank[position].B);
                    }

                    for (int q = 0; q < 100; q++)
                    {
                        var pX = (int)line[i].X;
                        var pY = (int)line[i].Y - q;
                        if (pX <= _zone.X || pY <= _zone.Y)
                        {
                            continue;
                        }

                        ColorComputing.PlotPixel(_buffer, pX, pY, _colorBank[position].R,
                            _colorBank[position].G,
                            _colorBank[position].B);
                    }
                }
                else if (endPoint.X < startPoint.X && endPoint.Y < startPoint.Y)
                {
                    for (int p = 0; p < 100; p++)
                    {
                        var pX = (int)line[i].X - p;
                        var pY = (int)line[i].Y;
                        if (pX <= _zone.X || pY <= _zone.Y)
                        {
                            continue;
                        }

                        ColorComputing.PlotPixel(_buffer, pX, pY, _colorBank[position].R,
                            _colorBank[position].G,
                            _colorBank[position].B);
                    }

                    for (int q = 0; q < 100; q++)
                    {
                        var pX = (int)line[i].X;
                        var pY = (int)line[i].Y + q;
                        if (pX >= _zone.Width + _zone.X || pY >= _zone.Height + _zone.Y)
                        {
                            continue;
                        }

                        ColorComputing.PlotPixel(_buffer, pX, pY, _colorBank[position].R,
                            _colorBank[position].G,
                            _colorBank[position].B);
                    }
                }
                //straight case
                else if (endPoint.X == startPoint.X)
                {
                    int lineX = (int)_zone.X;
                    int lineY = (int)line[i].Y;
                    ColorComputing.SetLineColor(_buffer, lineX, lineY, (int)_zone.Width, _colorBank[position].R,
                        _colorBank[position].G,
                        _colorBank[position].B);
                }
                else if (endPoint.Y == startPoint.Y)
                {
                    int lineY = (int)line[i].Y - 100;
                    int lineX = (int)line[i].X;
                    ColorComputing.SetLineColor(_buffer, lineX, lineY, 200, _colorBank[position].R,
                        _colorBank[position].G,
                        _colorBank[position].B);
                  
                }
            }

            offSet += line.Length;


            // right to left effect
            // hard part, animate using path??
            // color index change by x value
        }

       // _zone.UpdateFrame();
    }

    public void Init(LightingZone zone)
    {
        _zone = zone;
        _zone.UpdateFrameBuffer();

        config = (ColorPaletteConfiguration)_zone.LightingConfiguration;
        var pixelList = new List<Point>();

        config.Points = new List<Point>();
        config.Points.Add(new Point(_zone.Width / 2 + _zone.X, 0 + _zone.Y));
        config.Points.Add(new(_zone.Width / 2 + _zone.X, _zone.Height + zone.Y));


        int pointCount = 0;
        _pixelPath = new List<Point[]>();
        for (int p = 0; p < config.Points.Count - 1; p++)
        {
            var startPoint = config.Points[p];
            var endPoint = config.Points[p + 1];
            var points = ColorComputing
                .EnumerateLineNoDiagonalSteps((int)startPoint.X, (int)startPoint.Y, (int)endPoint.X, (int)endPoint.Y)
                .ToArray();
            _pixelPath.Add(points);
            pointCount += points.Length;
        }

        _colorBank = ColorComputing.GetColorColorBankfromPaletteWithFixedColorPerGap(config.Palette.Colors, pointCount)
            .ToArray();
    }

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        IsDisposed = true;
        GC.Collect();
    }

    public CapturingType CaptureType { get; set; }
}