namespace AmbinityCore.Models.Lighting.Zone.Configuration;

public class BreathingMotionConfiguration : IMotionConfiguration
{
    public BreathingMotionConfiguration()
    {
        
    }

    public MotionTypeEnum Type => MotionTypeEnum.Breathing;
    public string Icon => "breathing";

    public float BreathingSpeed { get; set; }
    public bool IsSystemSync { get; set; }
    

    public void UpdateConfig()
    {
        Update?.Invoke();
    }

    public void UpdateSpeed()
    {
        SpeedUpdate?.Invoke();
    }

    public void UpdateSystemSync()
    {
        SystemSyncUpdate?.Invoke();
    }
    public event Action Update;
    public event Action SpeedUpdate;
    public event Action SystemSyncUpdate;
}