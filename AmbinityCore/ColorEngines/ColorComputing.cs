using Avalonia;
using Avalonia.Media;
using Draw2D.Core.Graphic;

namespace AmbinityCore.LightingEngines;

public static class ColorComputing
{
     /// <summary>
 /// fast fill a block with color, this apply for straight path or progress
 /// </summary>
 /// <param name="block"></param>
 public static void SetBlockColor(FrameBuffer _imageBuffer, Rect block, byte redValue,
 byte greenValue, byte blueValue)
 {

     var lineColor = new byte[(int)block.Width * 4];
     for (int i = 0; i < lineColor.Length; i += 4)
     {
         lineColor[i] = blueValue;
         lineColor[i + 1] = greenValue;
         lineColor[i + 2] = redValue;
         lineColor[i + 3] = 255;

     }
     //copy array line by line
     for (int l = 0; l < block.Height; l++)
     {
         int start = (_imageBuffer.FrameWidth * 4) * ((int)block.Top + l) + (int)block.Left * 4;

         Array.Copy(lineColor, 0, _imageBuffer.PixelData, start, lineColor.Length);
     }
 }
 /// <summary>
 /// fast fill a line with color
 /// </summary>
 /// <param name="_imageBuffer"></param>
 /// <param name="block"></param>
 /// <param name="redValue"></param>
 /// <param name="greenValue"></param>
 /// <param name="blueValue"></param>
 public static void SetLineColor(FrameBuffer _imageBuffer, int lineX, int lineY, int lineWidth, byte redValue,
 byte greenValue, byte blueValue)
 {
     
     int start = (_imageBuffer.FrameWidth * 4) * lineY + lineX * 4;
     for (int i = 0; i < lineWidth * 4; i += 4)
     {
         _imageBuffer.PixelData[i +start] = blueValue;
         _imageBuffer.PixelData[i + 1 + start] = greenValue;
         _imageBuffer.PixelData[i + 2 + start] = redValue;
         _imageBuffer.PixelData[i + 3 + start] = 255;

     }
 }
 public static void PlotPixel(FrameBuffer _imageBuffer, int x, int y, byte redValue,
 byte greenValue, byte blueValue)
 {
     int offset = ((_imageBuffer.FrameWidth * 4) * y) + (x * 4);
     _imageBuffer.PixelData[offset] = blueValue;
     _imageBuffer.PixelData[offset + 1] = greenValue;
     _imageBuffer.PixelData[offset + 2] = redValue;
     // Fixed alpha value (No transparency)
     _imageBuffer.PixelData[offset + 3] = 255;
 }
 public static void SetBlockImage(byte[]_imageBuffer,Rect block, byte[]_imagesource)
 {
     int start = ((240 * 4) * ((int)block.Top)) + ((int)block.Left * 4);
     Array.Copy(_imagesource, 0, _imageBuffer, start, _imagesource.Length);
 }
 public static List<Color> GetColorGradient(Color from, Color to, int totalNumberOfColors)
 {
     if (totalNumberOfColors < 2)
     {
         throw new ArgumentException("Gradient cannot have less than two colors.", nameof(totalNumberOfColors));
     }
     var colorList = new List<Color>();
     double diffA = to.A - from.A;
     double diffR = to.R - from.R;
     double diffG = to.G - from.G;
     double diffB = to.B - from.B;

     var steps = totalNumberOfColors - 1;

     var stepA = diffA / steps;
     var stepR = diffR / steps;
     var stepG = diffG / steps;
     var stepB = diffB / steps;



     for (var i = 1; i < steps; ++i)
     {
         colorList.Add(Color.FromArgb(
              (byte)(c(from.A, stepA)),
              (byte)(c(from.R, stepR)),
              (byte)(c(from.G, stepG)),
              (byte)(c(from.B, stepB))));

         int c(int fromC, double stepC)
         {
             return (int)Math.Round(fromC + stepC * i);
         }
     }
     colorList.Add(to);
     colorList.Insert(0, from);
     return colorList;

 }
 public static IEnumerable<Color> GetColorGradientfromPaletteWithFixedColorPerGap(Color[] colorCollection)
 {
     var colors = new List<Color>();
     var colorPerGap = (int)(1024 / colorCollection.Length);

     for (int i = 0; i < colorCollection.Length - 1; i++)
     {
         var gradient = GetColorGradient(colorCollection[i], colorCollection[i + 1], colorPerGap);
         colors = colors.Concat(gradient).ToList();
     }
     var lastGradient = GetColorGradient(colorCollection[colorCollection.Length - 1], colorCollection[0], colorPerGap);
     colors = colors.Concat(lastGradient).ToList();
     return colors;

 }
 public static IEnumerable<Color> GetColorColorBankfromPaletteWithFixedColorPerGap(Color[] colorCollection, int length)
 {
     var colors = new List<Color>();
     var colorPerGap = (int)(length / colorCollection.Length);


     for (int i = 0; i < colorCollection.Length; i++)
     {
         var gradient = new Color[colorPerGap];
         Array.Fill(gradient, colorCollection[i]);
         colors = colors.Concat(gradient).ToList();
     }


     return colors;

 }
 public static IEnumerable<Point> EnumerateLineNoDiagonalSteps(int x0, int y0, int x1, int y1)
 {
     int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
     int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
     int err = dx + dy, e2;

     while (true)
     {
         yield return new Point(x0, y0);

         if (x0 == x1 && y0 == y1) break;

         e2 = 2 * err;

         // EITHER horizontal OR vertical step (but not both!)
         if (e2 > dy)
         {
             err += dy;
             x0 += sx;
         }
         else if (e2 < dx)
         { // <--- this "else" makes the difference
             err += dx;
             y0 += sy;
         }
     }
 }
}