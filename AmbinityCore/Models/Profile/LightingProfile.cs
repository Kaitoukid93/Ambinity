using System.Collections.ObjectModel;
using AmbinityCore.LightingEngines;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Lighting.Zone;
using CommunityToolkit.Mvvm.ComponentModel;


namespace AmbinityCore.Models.Profile;

public class LightingProfile : ObservableObject, IDisposable, ICollectableItem
{
    public event Action<ICollectableItem>? ItemNameChanged;
    public event Action<ICollectableItem>? ItemPinStatusChanged;
    public event Action<ICollectableItem>? ItemCheckStatusChanged;
    public LightingProfile()
    {
        Zones = new ObservableCollection<LightingZone>();
    }
    
    /// <summary>
    /// Name of the profile
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Display Icon of this profile
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// Describe the profile
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// get playing status of this profile
    /// </summary>

    public bool IsPlaying { get; private set; }

    /// <summary>
    /// get number of zones
    /// </summary>
    public int ZoneCount => Zones.Count;

    /// <summary>
    /// Unique ID of parrent category
    /// </summary>
    public Guid CategoryID { get; set; }

    /// <summary>
    /// unique ID of this profile
    /// </summary>
    public Guid ID { get; set; }
    /// <summary>
    /// Indicate Item is selected by user
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    /// Indicate Item is being edited by user
    /// </summary>
    public bool IsEditing { get; set; }

    /// <summary>
    /// Indicate Item is being checked by user
    /// </summary>
    public bool IsChecked { get; set; }

    /// <summary>
    /// Indicate Item is being Pinned to dashboard
    /// </summary>
    public bool IsPinned { get; set; }

    /// <summary>
    /// Store Local path of this item
    /// </summary>
    public string LocalPath { get; set; }

    /// <summary>
    /// Indicate if this is default item
    /// </summary>
    public bool Disposed { get; set; }
    public bool IsDefault { get; set; }
    public ObservableCollection<LightingZone> Zones { get; set; }

    private void Load()
    {
        //load avalable lighting zone
        //register lighting zone
    }

    /// <summary>
    /// save configuration to disk
    /// </summary>
    private void Save()
    {
    }

    /// <summary>
    /// add new lighting zone
    /// </summary>
    public void AddLightingZone(LightingZone zone)
    {
        //register zone
    }

    /// <summary>
    /// render activated child to canvas
    /// </summary>
    public void Render()
    {
        
    }

    public void TogglePlayPause()
    {
        IsPlaying = !IsPlaying;
    }
    public void RegisterZone(LightingZone zone)
    {
        //var engine = _colorEngineProvider.GetEngine(zone);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
            return;
        //clear lighting zone
        //suspend thread
        Disposed = true;
    }
    
    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}