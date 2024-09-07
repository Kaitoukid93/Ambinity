using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Repositories;
using AmbinityCore.Utils;
using AmbinityServer.OnlineItem;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace AmbinityCore.Models.Geography;

public abstract class Group : ObservableObject, ICollectableItem, IPositionAware
{
    public Group()
    {
        Child = new List<IPositionAware>();
    }

    public List<IPositionAware> Child { get; set; }
    public event Action<ICollectableItem>? ItemNameChanged;
    public event Action<ICollectableItem>? ItemPinStatusChanged;
    public event Action<ICollectableItem>? ItemCheckStatusChanged;
    public string Name { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public float Scale { get; set; }
    public float Rotation { get; set; }
    [JsonIgnore] public bool IsScalable { get; set; }
    [JsonIgnore] public bool IsRotatable { get; set; }
    [JsonIgnore] public bool IsSelectable { get; set; }
    [JsonIgnore] public bool IsResizeable { get; set; }
    [JsonIgnore] public bool IsDraggable { get; set; }
    [JsonIgnore] public bool IsDeleteable { get; set; }
    [JsonIgnore] public Rect Bound => new Rect(X, Y, Width, Height);
    public abstract ContainerFigure GetContainer();

    public abstract ContainerFigure Clone(float x, float y);
    

    public string GetDisplayName()
    {
        throw new NotImplementedException();
    }

    public Guid GroupID { get; set; }
    public void SetScale(float scale)
    {
        throw new NotImplementedException();
    }

    public void SetRotation(float angle)
    {
        throw new NotImplementedException();
    }

    public string Icon { get; }
    public bool IsSelected { get; set; }
    public bool IsEditing { get; set; }
    public bool IsChecked { get; set; }
    public bool IsPinned { get; set; }
    public string LocalPath { get; set; }

    public void Save()
    {
        JsonHelpers.WriteSimpleJson(this, LocalPath);
    }

    public CollectableItemRepository GetLocalRepository()
    {
        throw new NotImplementedException();
    }

    public OnlineItemRepository GetOnlineRerpository()
    {
        throw new NotImplementedException();
    }

    public void UpdateSizeByChild(bool withPoint)
    {
        var rects = new List<Rect>();
        foreach (var child in Child)
        {
            rects.Add(child.Bound);
        }

        var newBound = RectCalculation.GetBound(rects.ToArray());
        Width = (float)newBound.Width +5;
        Height = (float)newBound.Height +5;
        if (withPoint)
        {
            X = (float)newBound.Left -5;
            Y = (float)newBound.Top-5;
        }
    }
}