namespace Ambinity.Models;

public interface ISelectablePage
{
    int Order { get; }
    string PageName { get; }
    string Geometry { get; }
    object Content { get; }
        
}