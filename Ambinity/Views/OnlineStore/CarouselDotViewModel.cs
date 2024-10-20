using Ambinity.ViewModels;

namespace Ambinity.Views.OnlineStore;

public class CarouselDotViewModel : ViewModelBase
{
    public CarouselDotViewModel()
    {
        
    }
    private bool _selected;

    public bool Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            OnPropertyChanged();
        }
    }

    public string Icon => "dot";


    public string SelectedIcon => "selectedDot";
}