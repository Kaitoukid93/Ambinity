namespace AmbinityCore.Models.Device;

public class FanOutput
{
    public FanOutput(int speed, int index)
    {
        Speed = speed;
        Index = index;
    }
    public int Speed { get; set; }
    public int Index { get; set; }
}