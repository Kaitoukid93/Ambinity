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
    internal readonly partial struct Gooey : IPixelShader<Float4>
    {
        /// <summary>
        /// The current time since the start of the application.
        /// </summary>
        public readonly float time;

        // try running this through a kaleidoscope.

        private const float _Speed = 15.5f * 0.002f;
        private const float _Scale = 0.2f;
        private const float _Gamma = 0.15f;
        private const float _Colour = 0.15f;     // 0.01..0.3
        private const float _Brightness = 3.0f;
        private const float _Lacunarity = 1.6f;





        //iq noise fns
        static float hash(float n)
        {
            return Hlsl.Frac(Hlsl.Sin(n) * 43758.5453f);
        }

        static float noise(in Float3 x)
        {
            Float3 p = Hlsl.Floor(x);
            Float3 f = Hlsl.Frac(x);

            f = f * f * (3.0f - 2.0f * f);
            float n = p.X + p.Y * 57.0f + 113.0f * p.Z;
            return Hlsl.Lerp(Hlsl.Lerp(Hlsl.Lerp(hash(n + 0.0f), hash(n + 1.0f), f.X),
                                       Hlsl.Lerp(hash(n + 57.0f), hash(n + 58.0f), f.X), f.Y),
                             Hlsl.Lerp(Hlsl.Lerp(hash(n + 113.0f), hash(n + 114.0f), f.X),
                                       Hlsl.Lerp(hash(n + 170.0f), hash(n + 171.0f), f.X), f.Y), f.Z);
        }



        //x3
        static Float3 noise3(in Float3 x)
        {
            return new Float3(noise(x + new Float3(123.456f, .567f, .37f)),
                        noise(x + new Float3(.11f, 47.43f, 19.17f)),
                        noise(x));
        }

        static Float3x3 rotation(float angle, Float3 axis)
        {
            float s = Hlsl.Sin(-angle);
            float c = Hlsl.Cos(-angle);
            float oc = _Colour - c;
            Float3 sa = axis * s;
            Float3 oca = axis * oc;
            return new Float3x3(
                oca.X * axis + new Float3(c, -sa.Z, sa.Y),
                oca.Y * axis + new Float3(sa.Z, c, -sa.X),
                oca.Z * axis + new Float3(-sa.Y, sa.X, c));
        }

        // https://code.google.com/p/fractalterraingeneration/wiki/Fractional_Brownian_Motion
        static Float3 fbm(Float3 x, float H, float L)
        {
            Float3 v = new Float3(0f,0f,0f);
            float f = 1.0f;

            for (int i = 0; i < 7; i++)
            {
                float w = Hlsl.Pow(f, -H);
                v += noise3(x) * w;
                x *= L;
                f *= L;
            }
            return v;
        }

        public Float4 Execute()
        {
            Float2 uv = (Float2)ThreadIds.XY / DispatchSize.XY;
            uv.X *= DispatchSize.X / DispatchSize.Y;

            float t = time * _Speed;

            uv *= 1.0f + 0.25f * Hlsl.Sin(t * 10.0f);  // drift scale in and out a little

            Float3 p = new Float3(uv * _Scale, t);                   //coordinate + slight change over time

            Float3 axis = 4.0f * fbm(p, 0.5f, _Lacunarity);              //random fbm axis of rotation

            Float3 colorVec = 0.5f * 5.0f * fbm(p * 0.3f, 0.5f, _Lacunarity);  //random base color
            Float3 norm = Hlsl.Normalize(axis);
            colorVec = Hlsl.Mul(colorVec, rotation(3.0f * Hlsl.Length(axis), norm));
            colorVec *= 0.05f;

            colorVec = Hlsl.Pow(colorVec, _Gamma);         //gamma
            return new Float4(_Brightness * colorVec * colorVec, 1.0f);
        }

    }
}