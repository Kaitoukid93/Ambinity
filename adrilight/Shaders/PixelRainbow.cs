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
    internal readonly partial struct PixelRainbow : IPixelShader<Float4>
    {
        /// <summary>
        /// The current time since the start of the application.
        /// </summary>
        public readonly float time;

        // try running this through a kaleidoscope.

        private Float4 hue(float v)
        {
            return (.6f + .6f * Hlsl.Cos(6.3f * (v) + new Float4(0f, 23f, 21f, 0f)));
        }

        public Float4 Execute()
        {
            float t = time;
            Float2 uv = Hlsl.Floor((Float2)ThreadIds.XY / DispatchSize.X * 12.0f) / 12.0f;
            return (hue(uv.X + uv.Y / 3.0f + t * 0.5f));
        }
    }
}