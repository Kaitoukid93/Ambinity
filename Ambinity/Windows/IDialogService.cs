using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;

namespace Ambinity.Windows;

public interface IDialogService
{
    Task ShowInputDialog(InputDialogContentViewModel vm, string title, string primaryButtonText,
        string closeButtonText);
    Task ShowCreateNewProfileDialog(NewProfileDialogContentViewModel vm);
    Task ShowCreateNewDeviceDialog(NewDeviceDialogContentViewModel vm);
    Task ShowDownloadDialog(DownloadDialogViewModel vm,bool showCancelButton );
    Task ShowConfirmationDialog(ConfirmationDialogContentViewModel vm, string title, string primaryButtonText,
        string closeButtonText);
    Task ShowWindowDialog(WindowDialogViewModelBase vm,string title,string primaryButtonText,
        string closeButtonText);
    Task ShowErrorDialog(ErrorDialogViewModel vm,string title,
        string closeButtonText);
    Task ShowLoadingDialog(LoadingDialogViewModel vm,string title);
}
