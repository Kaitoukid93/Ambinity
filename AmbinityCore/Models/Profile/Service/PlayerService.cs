namespace AmbinityCore.Models.Profile.Service;
using System.Reflection.Metadata;
using System.Collections.Concurrent;

public class PlayerService : IPlayerService
{
    private readonly LightingProfileDecoder _decoder;

    private Guid? _currentPlayingId;

    public Guid? CurrentPlayingId => _currentPlayingId;

    public event Action<Guid?>? PlayingChanged;

    public PlayerService(LightingProfileDecoder decoder)
    {
        _decoder = decoder;
    }

    public bool IsPlaying(Guid id)
    {
        return _currentPlayingId == id;
    }

    public async Task PlayAsync(Guid id)
    {
        if (_currentPlayingId == id)
            return;

        // stop current
        await _decoder.StopAsync();

        _currentPlayingId = id;
        PlayingChanged?.Invoke(_currentPlayingId);
    }

    public async Task StopAsync()
    {
        if (_currentPlayingId == null)
            return;

        await _decoder.StopAsync();

        _currentPlayingId = null;
        PlayingChanged?.Invoke(_currentPlayingId);
    }

    public async Task ToggleAsync(Guid id)
    {
        if (_currentPlayingId == id)
            await StopAsync();
        else
            await PlayAsync(id);
    }
}
