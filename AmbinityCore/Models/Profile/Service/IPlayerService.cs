namespace AmbinityCore.Models.Profile.Service;
public interface IPlayerService
{
    Guid? CurrentPlayingId { get; }

    bool IsPlaying(string id);

    Task PlayAsync(string id);
    Task StopAsync();
    Task ToggleAsync(string id);

    event Action<string?> PlayingChanged;
}
