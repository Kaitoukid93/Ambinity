using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.Windows;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Toolbar;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.ViewModels;

public class CollectableItemsViewModel : ViewModelBase
{
    public CollectableItemsViewModel(CollectableItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
        AvailableTools = new ObservableCollection<IToolbarItem>();
        Items = new ObservableCollection<ICollectableItem>();
        _progress = new Progress<int>(percent =>
        {
            LoadingPercent = percent;
        });
        CommandSetup();
    }

    private CollectableItemRepository _itemRepository;
    private bool _isLoadingValue;
    public bool ShowToolBar => AvailableTools.Count > 0;
    public ObservableCollection<IToolbarItem> AvailableTools { get; set; }
    public ObservableCollection<ICollectableItem> Items { get; set; }
    private int _uniformGridRowNumber = 1;

    /// <summary>
    /// calculating grid row and columns
    /// </summary>
    public int UniformGridRowNumber
    {
        get { return _uniformGridRowNumber; }
        set
        {
            _uniformGridRowNumber = value;
            RaisePropertyChanged(nameof(UniformGridRowNumber));
        }
    }
    private IProgress<int> _progress;
    private int _loadingPercent;
    private bool _progressBarVisibility;
    public bool ProgressBarVisibility {
        get
        {
            return _progressBarVisibility;
        }
        set
        {
            _progressBarVisibility = value;
            if (!value)
            {
                _progress.Report(0);
            }
            RaisePropertyChanged(nameof(ProgressBarVisibility));
        }
    }

    /// <summary>
    /// Current Loading status
    /// </summary>
    public int LoadingPercent
    {
        get { return _loadingPercent; }
        set
        {
            if (_loadingPercent != value)
            {
                _loadingPercent = value;
                RaisePropertyChanged(nameof(LoadingPercent));
            }
        }
    }

    public void Init()
    {
    }

    private void CommandSetup()
    {
        DeleteItemsCommand = new AsyncRelayCommand(ExecuteDeleteItem);
    }
    /// <summary>
    /// Async load items from disk
    /// </summary>
    /// <returns></returns>
    public async Task Load()
    {
        if (_isLoadingValue)
            return;
        _isLoadingValue = true;
        ProgressBarVisibility = true;
        await Task.Delay(100);
        //update items from disk
        await Task.Run(() => _itemRepository.LoadFromDisk());
        int count = 0;
        //calculate grid rows and collumns
        UniformGridRowNumber = (int)Math.Ceiling(_itemRepository.Items.Count / 4d);
        var step = 100 / (_itemRepository.Items.Count / 4);
        var percent = step;
        foreach (var value in _itemRepository.Items)
        {
            Items.Add(value);
            count++;

            if (count % 4 == 0)
            {
                await Task.Delay(150);
                if (LoadingPercent > 100)
                    LoadingPercent = 100;
                _progress.Report(percent += step);
            }
        }
        UpdateTools();
        await Task.Delay(100);
        _isLoadingValue = false;
        ProgressBarVisibility = false;
    }
    private void UpdateTools()
    {
        //clear Tool
        AvailableTools?.Clear();
        var selectedItems = Items.Where(d => d.IsChecked).ToList();
        if (selectedItems != null && selectedItems.Count > 0)
            AvailableTools.Add(DeleteTool());
        RaisePropertyChanged(nameof(ShowToolBar));

    }
    private IToolbarItem DeleteTool()
    {
        return new ButtonToolbarItem() {
            Name = "Delete",
            ToolTip = "Delete Selected Items",
            Icon = "remove",
            Command = DeleteItemsCommand

        };
    }

    private async Task ExecuteDeleteItem()
    {
        //await show dialog
        UpdateTools();
    }
    public ICommand DeleteItemsCommand { get; set; }
}