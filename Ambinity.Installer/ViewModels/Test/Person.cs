namespace Ambinity.Installer.ViewModels.Test;

public class Person
{
    public Person(string name, string age, string address)
    {
        Name = name;
        Age = age;
        Address = address;
    }

    public Person()
    {
        
    }
    public string Name { get; set; }
    public string Age { get; set; }
    public string Address { get; set; }
}