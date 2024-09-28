namespace AmbinityCore.Models.Ultilities;

public class ProgressInformation
{
    public ProgressInformation(string info, int progress)
    {
        Info = info;
        Progress = progress;
    }
    public string Info { get; set; }
    public int Progress { get; set; }
}