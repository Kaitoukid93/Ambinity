using System.Diagnostics;
using System.Windows.Input;
using Ambinity.ViewModels;
using Ambinity.Views.OnlineStore;
using AmbinityServer.OnlineItem;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.Screens.Home;

public class HyperLinkHomeViewModel : ViewModelBase
{
    private OnlineItem _item;
    private readonly ThumbnailService _thumbnailService;

    public HyperLinkHomeViewModel(OnlineItem item,ThumbnailService thumbnailService)
    {
        _thumbnailService = thumbnailService;
        _item = item;
        Name = _item.Name;
        Description = _item.Description;
        OpenHyperLinkCommand = new RelayCommand(OpenHyperLink);
    }

    private void OpenHyperLink()
    {
        Process.Start(new ProcessStartInfo(_item.Hyperlink) { UseShellExecute = true });
    }

    public string Name { get; set; } 
    public string Description { get; set; }
    public ICommand OpenHyperLinkCommand { get; }
    
}