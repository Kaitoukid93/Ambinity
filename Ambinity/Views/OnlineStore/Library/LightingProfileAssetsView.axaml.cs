using System.Threading.Tasks;
using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Ambinity.Views.OnlineStore.Library;

public partial class LightingProfileAssetsView : UserControl
{
    public LightingProfileAssetsView()
    {
        InitializeComponent();
    }
    private bool _isLoadingMoreItems;
    private async void InputElement_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        //bottom reached,load more
        if(_isLoadingMoreItems)
            return;
        if (e.Delta.Y == -1)
        {
            _isLoadingMoreItems = true;
            var vm = this.DataContext as LightingProfileAssetsViewModel;
            await Task.Run(()=>vm.LoadMoreIfAvailable());
            _isLoadingMoreItems = false;
        }
        
    }
}