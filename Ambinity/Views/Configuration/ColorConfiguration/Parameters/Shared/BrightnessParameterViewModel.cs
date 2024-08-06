namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class BrightnessParameterViewModel : ParameterViewModelBase
{
    public BrightnessParameterViewModel()
    {
        Header = "Brightness";
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