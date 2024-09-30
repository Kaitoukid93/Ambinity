using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using Ambinity.Views.LayoutEditor;
using AmbinityCore.Repositories;

namespace Ambinity.Views.Screens.DeviceLayout.Library;

public class DeviceLayoutsLibraryViewModel : LibraryViewModelBase
{
    public DeviceLayoutsLibraryViewModel(AmbinityDeviceLayoutRepository layoutRepository,
        AmbinityDeviceOnlineRepository onlineRepository,
        DeviceLayoutAssetsViewModel layoutAssetsViewModel) : base(
        layoutRepository, onlineRepository, layoutAssetsViewModel)
    {
    }
}