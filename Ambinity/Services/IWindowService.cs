using System;
using System.Threading.Tasks;
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
    ///     Gets the current window of the application
    /// </summary>
    /// <returns>The current window of the application</returns>
    Window? GetCurrentWindow();
}