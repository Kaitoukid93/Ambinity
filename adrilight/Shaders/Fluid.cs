using ComputeSharp;

namespace adrilight.Shaders
{
    /// <summary>
    /// A shader creating an abstract and colorful animation.
    /// Ported from <see href="https://www.shadertoy.com/view/WtjyzR"/>.
    /// <para>Created by Benoit Marini.</para>
    /// <para>License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.</para>
    /// </summary>
    [AutoConstructor]
    internal readonly partial struct Fluid : IPixelShader<Float4>
    {
        /// <summary>
        /// The current time since the start of the application.
        /// </summary>
        public readonly float time;

        // try running this through a kaleidoscope.


        float superellipse(Float2 uv, Float2 o, float r, float n)
        {
            float res = Hlsl.Pow(Hlsl.Abs((uv.X - o.X) / r), n) + Hlsl.Pow(Hlsl.Abs((uv.Y - o.Y) / r), n);
            return res <= 1.0f? Hlsl.Sqrt(1.0f- res) : .0f;
        }

        Float3 putPixel(Float2 uv)
        {
            return superellipse(Hlsl.Frac(uv), new Float2(.5f,.5f), .5f, 3.5f) * new Float3(.1f, .9f, .07f);
        }

        public Float4 Execute()
        {
            float tm = time;
            Float2 R = (Float2)(DispatchSize.XY);
            Float2 uv = (2.0f* (Float2)ThreadIds.XY - R) / R.Y * 25f,
                fuv = Hlsl.Floor(uv) + .5f,
                  t = new Float2(Hlsl.Sin(tm), Hlsl.Cos(tm)) * 25f/ 2.0f,
                 o1 = new Float2(0, t.X),
                 o2 = new Float2(1.7f * t.X, 0f),
                 o3 = 2.0f* t.YX;
            Float3 l = new Float3(Hlsl.Distance(o1, fuv).X, Hlsl.Distance(o2, fuv).X, Hlsl.Distance(o3, fuv).X),
                 g = 25f * new Float3(.5f / (l.X * l.X), 1.0f/ (l.Y * l.Y), .75f / (l.Z * l.Z));
            //Float4 color= new Float4(0,0,0,0);
            //color.XYZ = g.X + g.Y + g.Z > 15.0f/ 25f ? putPixel(uv) : new Float3(0,0,0);
            if (g.X + g.Y + g.Z > 15.0f / 25f)
            {
                return new Float4(putPixel(uv), 1.0f);

            }
            else return new Float4(0, 0, 0, 1.0f);
            
        }
    }
}