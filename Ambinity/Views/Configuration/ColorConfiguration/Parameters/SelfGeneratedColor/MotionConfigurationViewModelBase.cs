using Ambinity.Views.LayoutEditor;
using AmbinityCore.Models.Lighting.Zone.Configuration;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public abstract class MotionConfigurationViewModelBase : FlyoutContentViewModelBase
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public MotionTypeEnum Type { get; set; }
    public IMotionConfiguration Configuration { get; set; }
    public bool ShowSettingsButton { get; set; }
}