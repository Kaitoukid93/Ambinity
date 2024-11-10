using System.Collections.Generic;

namespace Ambinity.Installer.ViewModels.Test;

public class PeopleRepository
{
    public PeopleRepository()
    {
        //init repo such as load from local db or network
        // Khởi tạo list people hoặc tải từ network hoặc local db
        People = new List<Person>()
        {
            new Person("Zone", "22", "Hanoi"),
            new Person("Chloe", "32", "HCM")
        };
    }
    public List<Person> People { get; set; }
}