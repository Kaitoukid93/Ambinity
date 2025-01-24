using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Ambinity.Views.Screens.DeviceLayout.Library;

public partial class DeviceLayoutAssetsView : UserControl
{
    private bool _isLoadingMoreItems;
    public DeviceLayoutAssetsView()
    {
        InitializeComponent();
    }


    private async void InputElement_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        //bottom reached,load more
        if (_isLoadingMoreItems)
            return;
        if (e.Delta.Y == -1)
        {
            _isLoadingMoreItems = true;
            var vm = this.DataContext as DeviceLayoutAssetsViewModel;
            await Task.Run(() => vm.LoadMoreIfAvailable());
            _isLoadingMoreItems = false;
        }
    }
    
}