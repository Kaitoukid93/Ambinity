using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ambinity.Installer.ViewModels.Test;

public class MainWindowViewModel : ViewModelBase
{
    private readonly PeopleRepository _peopleRepo;

    //dependency injection recommended
    public MainWindowViewModel( PeopleRepository peopleRepository)
    {
        _peopleRepo = peopleRepository;
        PeopleSource = new ObservableCollection<PersonViewModel>();
        Init();
    }

    public void Init()
    {
        LoadData();
    }
public ObservableCollection<PersonViewModel> PeopleSource { get; set; }
    private void LoadData()
    {
        //load people database
        PeopleSource?.Clear();
        foreach (var person in _peopleRepo.People)
        {
            var personVm = new PersonViewModel(person);
            RegisterPerson(personVm);
            PeopleSource.Add(personVm);
        }
    }
    private void RegisterPerson(PersonViewModel person)
    {
        person.Checked += OnPersonChecked;
    }

    private void OnPersonChecked(Person person)
    {
        Greeting = "Hola" + " " + person.Name;
    }
    private string _greeting = "Hola";
    public string Greeting
    {
        get => _greeting;
        set
        {
            _greeting = value;
            OnPropertyChanged();
        }
    }
}