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
    internal readonly partial struct Falling : IPixelShader<Float4>
    {
        /// <summary>
        /// The current time since the start of the application.
        /// </summary>
        public readonly float time;

        // try running this through a kaleidoscope.
        float Cell(Float2 c)
        {
            Float2 uv = Hlsl.Frac(c);
            c -= uv;
            return (3.0f - Hlsl.Length(uv)) * Hlsl.Step(Hlsl.Frac(Hlsl.Sin(c.X + c.Y * 100.0f) * 1000.0f), 0.04f); // 3.0 :: star width and fade strength, 0.04 :: star count
        }

        public Float4 Execute()
        {
            Float2 projection = ThreadIds.XY / new Float2(120.0f, 120.0f); // TEXCOORD divided by image resolution
            float projX = Hlsl.Frac(projection.X) / 1.0f; // XY direction divided by scale of stars along X
            float projY = Hlsl.Pow(projection.Y, 0.014f); // 0.014 :: speed1 of stars 
            float t = -time; //reverse or forward speed
            Float3 color = new Float3(0, 0, 0);

            for (int rgb = 0; rgb < 3; rgb++)
            {
                t -= 0.02f; // color shift distance
                Float2 coord = new Float2(projY, projX) * 256.0f; // *512 :: star count, adjust star-count and scale for scaling along Y axis 
                Float2 delta = new Float2(time * 7.0f, 0.0f); // time*7.0 :: speed2 of stars, adjust both^^ speeds for length of stars
                float c = Cell(coord -= delta);
                c += Cell(coord -= delta);
                color[rgb] = c * projection.Y * 16.0f; // fade distance at bottom of screen
            }

            return new Float4(color, 1.0f);
        }


    }
}