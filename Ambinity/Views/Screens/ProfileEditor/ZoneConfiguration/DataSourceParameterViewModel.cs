using System.Collections.Generic;
using AmbinityCore.Models.Collection;

namespace Ambinity.Views.Screens.ProfileEditor.ZoneConfiguration;

public class DataSourceParameterViewModel : ParameterViewModelBase
{
    public DataSourceParameterViewModel()
    {
        Datarepositories = new List<CollectableItemRepository>();
    }
    public List<CollectableItemRepository> Datarepositories { get; set; }
}