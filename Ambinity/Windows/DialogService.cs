using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;

namespace Ambinity.Windows;

public class DialogService : IDialogService
{
    /// <summary>
    /// show input dialog for rename or add new stuff
    /// </summary>
    public async Task ShowInputDialog(InputDialogContentViewModel vm, string title, string primaryButtonText,
        string closeButtonText)
    {
        var dialog = new ContentDialog()
        {
            Title = title,
            PrimaryButtonText = primaryButtonText,
            IsSecondaryButtonEnabled = false,
            CloseButtonText = closeButtonText
        };
        vm.Init(dialog);
        dialog.Content = new InputDialogContent()
        {
            DataContext = vm
        };

        var result = await dialog.ShowAsync();
    }

    public async Task ShowDownloadDialog(DownloadDialogViewModel vm, Window owner, bool showCancelButton)
    {
        var td = new TaskDialog
        {
            Title = vm.Title,
            ShowProgressBar = true,
            IconSource = new SymbolIconSource { Symbol = Symbol.Download },
            SubHeader = "Downloading",
            Content = vm.Description,
        };
        if (showCancelButton)
            td.Buttons = new List<TaskDialogButton>() { TaskDialogButton.CancelButton };
        vm.Init(td);
        td.XamlRoot = owner;
        var result = await td.ShowAsync();
    }

    public async Task ShowDeleteDialog(DeleteDialogContentViewModel vm, string title, string primaryButtonText,
        string closeButtonText)
    {
        var dialog = new ContentDialog()
        {
            Title = title,
            PrimaryButtonText = primaryButtonText,
            IsSecondaryButtonEnabled = false,
            CloseButtonText = closeButtonText
        };
        vm.Init(dialog);
        dialog.Content = new DeleteDialogContent()
        {
            DataContext = vm
        };

        var result = await dialog.ShowAsync();
    }

    public async Task ShowWindowDialog(WindowDialogViewModelBase vm,string title, string primaryButtonText, string closeButtonText)
    {
        string name = vm.GetType().FullName!.Split('`')[0].Replace("ViewModel", "View");
        Type? type = vm.GetType().Assembly.GetType(name);

        if (type == null)
            throw new Exception($"Failed to find a usercontrol named {name}.");

        if (!type.IsAssignableTo(typeof(UserControl)))
            throw new Exception($"Type {name} is not a usercontrol.");

        UserControl content = (UserControl)Activator.CreateInstance(type)!;
        content.DataContext = vm;
        var dialog = new ContentDialog()
        {
            Title = title,
            PrimaryButtonText = primaryButtonText,
            IsSecondaryButtonEnabled = false,
            CloseButtonText = closeButtonText
        };
        vm.Init(dialog);
        dialog.Content = content;
        
        var result = await dialog.ShowAsync();
    }
    
}