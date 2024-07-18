using System.Threading.Tasks;

namespace Ambinity.Windows;

public interface IDialogService
{
    Task ShowInputDialog(InputDialogContentViewModel vm, string title, string primaryButtonText,
        string closeButtonText);
}