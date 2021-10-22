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
    internal readonly partial struct MetaBalls : IPixelShader<Float4>
    {
        /// <summary>
        /// The current time since the start of the application.
        /// </summary>
        public readonly float time;

        /// <summary>
        /// The total number of layers for the final animation.
        /// </summary>
        

        /// <summary>
        /// The number of iterations to calculate a texel value.
        /// </summary>


        /// <summary>
        /// Makes some magic happen.
        /// </summary>
        private const float speed = 2f;
        private const float Count = 10f;
        
        // hash functions from https://www.shadertoy.com/view/4sjGD1
        private float hash(float x)
        {
            return Hlsl.Frac(Hlsl.Sin(x) * 43758.5453f) * 2.0f - 1.0f;
        }
       private Float2 hashPosition(float x)
        {
            return new Float2(
                Hlsl.Floor(hash(x) * 3.0f) * 32.0f + 16.0f,
                Hlsl.Floor(hash(x * 1.1f) * 2.0f) * 32.0f + 16.0f
            );
        }
        /// <inheritdoc/>
        public Float4 Execute()
        {
            float t = time * speed;
            float falloff = 1.3f;
            Float3 metaBalls;
            float d = 0f;
            Float2 screen;
            float c = 0f;
            Float2 res = DispatchSize.XY;
            for (float i = .0f; i < Count; i++)
            {
                Float2 pos = hashPosition(i) +  (t * (1f + i));

                screen = Hlsl.Floor(Hlsl.Fmod(pos / res, 2f));

                // remap 0..1 to -1 to 1 to avoid branching in next line.
                Float2 dir = screen * 2f - 1f;
                Float2 finalPos = res * (1f - screen) + dir * Hlsl.Fmod(pos, res);

                //int idx = (int)(i);
                metaBalls = new Float3(finalPos, Hlsl.Abs(i));
                metaBalls.Z = res.Y / 200f * i * .25f;
                
                d = metaBalls.Z /Hlsl.Distance(metaBalls.XY, ThreadIds.XY).X;
                c += Hlsl.Pow(d, falloff);
            }

            return new Float4(new Float3(.0f, c, .0f), 1f);
        }
    }
}