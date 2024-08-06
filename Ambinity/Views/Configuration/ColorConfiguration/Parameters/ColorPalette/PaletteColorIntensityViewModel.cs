namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class PaletteColorIntensityViewModel : ParameterViewModelBase
{
    public PaletteColorIntensityViewModel()
    {
        Header = "Intensity";
    }
    public string Header { get; set; }
    private int _value;

    public int Value
    {
        get => _value;
        set
        {
            _value = value;
            OnPropertyChanged();
        }
    }
}