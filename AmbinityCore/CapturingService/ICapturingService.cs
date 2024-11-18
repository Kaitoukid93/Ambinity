namespace AmbinityCore.CapturingService;

public interface ICapturingService
{
    void Init();
    void Dispose();
    bool IsEnabled { get; }
    
}