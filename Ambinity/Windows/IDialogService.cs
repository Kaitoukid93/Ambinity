using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;

namespace Ambinity.Windows;

public interface IDialogService
{
    Task ShowInputDialog(InputDialogContentViewModel vm, string title, string primaryButtonText,
        string closeButtonText);
    Task ShowDownloadDialog(DownloadDialogViewModel vm, Window owner,bool showCancelButton );
}