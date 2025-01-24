using System.Threading.Tasks;
using Ambinity.Views.Screens.DeviceLayout.Library;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public partial class ColorPaletteAssetsView : UserControl
{
    private bool _isLoadingMoreItems;
    public ColorPaletteAssetsView()
    {
        InitializeComponent();
    }
    private async void InputElement_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        //bottom reached,load more
        if(_isLoadingMoreItems)
            return;
        if (e.Delta.Y == -1)
        {
            _isLoadingMoreItems = true;
            var vm = this.DataContext as ColorPaletteAssetsViewModel;
            await Task.Run(()=>vm.LoadMoreIfAvailable());
            _isLoadingMoreItems = false;
        }
        
    }
}