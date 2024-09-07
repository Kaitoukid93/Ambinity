using System;
using System.Threading.Tasks;
using Ambinity.Builders;
using Avalonia.Controls;

namespace Ambinity.Services;

public interface IWindowService
{
    Window ShowWindow<TViewModel>(out TViewModel viewModel);

    /// <summary>
    /// Show View from a given ViewModel
    /// </summary>
    /// <param name="viewModel"></param>
    /// <returns></returns>
    Window ShowWindow(object viewModel);

    /// <summary>
    /// Show View from a given ViewModel at specific screen
    /// </summary>
    /// <param name="viewModel"></param>
    /// <param name="monitor"></param>
    /// <returns></returns>
    Window ShowWindow(object viewModel, int monitor);

    /// <summary>
    ///     Gets the current window of the application
    /// </summary>
    /// <returns>The current window of the application</returns>
    Window? GetCurrentWindow();

    /// <summary>
    /// Open a file picker dialog, thanks to artemis
    /// </summary>
    /// <returns></returns>
    OpenFileDialogBuilder CreateOpenFileDialog();

    SaveFileDialogBuilder CreateSaveFileDialog();
}