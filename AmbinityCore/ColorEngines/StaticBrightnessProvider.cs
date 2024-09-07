using AmbinityCore.Models.Lighting.Zone.Configuration;

namespace AmbinityCore.LightingEngines;

public class StaticBrightnessProvider : IBrightnessProvider
{
    private static byte[] _staticBrightness = new byte[32];
    public StaticBrightnessProvider()
    {
        for (int i = 0; i < 32; i++)
        {
            _staticBrightness[i] = 255;
        }
    }

    public void Init(IMotionConfiguration config)
    {
        throw new NotImplementedException();
    }

    public void GetBrightness(float[] reusableArray)
    {
        for (int i = 0; i < reusableArray.Length; i++)
        {
            reusableArray[i] = 255;
        }
    }

    public void Activate()
    {
       
    }

    public void Deactivate()
    {
    }

}