using System;

namespace Ambinity.Installer.ViewModels.Test;

public class PersonViewModel : ViewModelBase
{
    private readonly Person _person;
    public event Action<Person> Checked;
    public PersonViewModel(Person person)
    {
        _person = person;
    }

    private bool _isChecked;
    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            _isChecked = value;
            if (value)
            {
                Checked?.Invoke(_person);
            }
            OnPropertyChanged();
        }
    }

}