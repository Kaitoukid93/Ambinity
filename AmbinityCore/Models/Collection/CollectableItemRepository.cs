using System.Collections.ObjectModel;
using System.ComponentModel;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Toolbar;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmbinityCore.Models.Collection;

public abstract class CollectableItemRepository : ObservableObject
{
    public event Action<ICollectableItem> ItemNameChaned;
    public event Action<ICollectableItem> ItemPinStatusChanged;
    public event Action<ICollectableItem> ItemCheckStatusChanged;
    public event Action<string> OnInitialized;

    #region Construct

    public CollectableItemRepository()
    {
        Items = new ObservableCollection<ICollectableItem>();
    }
    public CollectableItemRepository(string name)
    {
        Name = name;
        Items = new ObservableCollection<ICollectableItem>();
    }

    public string LocalFolderPath { get; set; }

    public virtual void Init()
    {
        if (!Directory.Exists(LocalFolderPath))
        {
            CreateDefault();
            SaveToDisk();
            return;
        }

        LoadFromDisk();
        OnInitialized?.Invoke(Name);
    }

    public virtual void CreateDefault()
    {
    }

    public virtual void LoadFromDisk()
    {
    }

    public virtual void SaveToDisk()
    {
        if(LocalFolderPath==null)
            return;
        if (!Directory.Exists(LocalFolderPath))
            Directory.CreateDirectory(LocalFolderPath);
        lock (Items)
        {
            foreach (var item in Items)
            {
                var localPath = Path.Combine(LocalFolderPath, item.Name + ".json");
                JsonHelpers.WriteSimpleJson(item, localPath);
            }
        }
    }

    public virtual void SaveToDisk(ICollectableItem item)
    {
        if (!Directory.Exists(LocalFolderPath))
            Directory.CreateDirectory(LocalFolderPath);
        lock (item)
        {
            var localPath = Path.Combine(LocalFolderPath, item.Name + ".json");
            JsonHelpers.WriteSimpleJson(item, localPath);
        }
        
    }

    #endregion

    #region Properties

    public ObservableCollection<ICollectableItem> Items { get; set; }
    private ICollectableItem _selectedItem;

    public ICollectableItem SelectedItem
    {
        get => _selectedItem;
        set
        {
            _selectedItem = value;
            OnPropertyChanged();
        }
    }

    public string Name { get; set; }

    #endregion


    #region Methods

    public void AddItem(ICollectableItem item)
    {
        RegisterItem(item);
        Items.Add(item);
    }

    public void InsertItem(ICollectableItem item)
    {
        RegisterItem(item);
        Items.Insert(0, item);
    }

    public void RemoveSelectedItems()
    {
        var selectedItems = Items.Where(i => i.IsChecked).ToList();
        //try to remove local path
        foreach (var item in selectedItems)
        {
            item.ItemCheckStatusChanged -= OnItemCheckStatusChanged;
            item.ItemNameChanged -= OnItemNameChanged;
            item.ItemPinStatusChanged -= OnItemPinStatusChanged;
            item.PropertyChanged -= OnItemPropertyChanged;
            Items.Remove(item);
        }
    }

    /// <summary>
    /// Register when new item got insert or add
    /// </summary>
    private void RegisterItem(ICollectableItem item)
    {
        item.ItemCheckStatusChanged += OnItemCheckStatusChanged;
        item.ItemNameChanged += OnItemNameChanged;
        item.ItemPinStatusChanged += OnItemPinStatusChanged;
        item.PropertyChanged += OnItemPropertyChanged;
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        SaveToDisk(sender as ICollectableItem);
    }

    private void OnItemNameChanged(ICollectableItem item)
    {
        ItemNameChaned?.Invoke(item);
    }

    private void OnItemPinStatusChanged(ICollectableItem item)
    {
        ItemPinStatusChanged?.Invoke(item);
    }

    private void OnItemCheckStatusChanged(ICollectableItem item)
    {
        ItemCheckStatusChanged?.Invoke(item);
    }

    public void ResetSelectionStage()
    {
        foreach (var item in Items)
            if (item.IsChecked)
            {
                item.IsChecked = false;
            }
    }

    #endregion
}