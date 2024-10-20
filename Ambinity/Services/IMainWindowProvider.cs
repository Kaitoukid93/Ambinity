using System;
using AmbinityCore.Models.Profile;

namespace Ambinity.Services;

public interface IMainWindowProvider
{
    /// <summary>
    ///     Gets a boolean indicating whether the main window is currently open
    /// </summary>
    bool IsMainWindowOpen { get; }

    /// <summary>
    ///     Gets a boolean indicating whether the main window is currently focused
    /// </summary>
    bool IsMainWindowFocused { get; }

    /// <summary>
    ///     Opens the main window
    /// </summary>
    void OpenMainWindow();

    /// <summary>
    ///     Opens the main window and navigate to profile editor
    /// </summary>
    void OpenMainWindow(LightingProfile profile);

    /// <summary>
    /// Opens the main window and navigate to screen with index
    /// </summary>
    /// <param name="screenIndex"></param>
    void OpenMainWindow(int screenIndex);

    /// <summary>
    ///     Closes the main window
    /// </summary>
    void CloseMainWindow();

    /// <summary>
    ///     Occurs when the main window has been opened
    /// </summary>
    public event EventHandler? MainWindowOpened;

    /// <summary>
    ///     Occurs when the main window has been closed
    /// </summary>
    public event EventHandler? MainWindowClosed;

    /// <summary>
    ///     Occurs when the main window has been focused
    /// </summary>
    public event EventHandler? MainWindowFocused;

    /// <summary>
    ///     Occurs when the main window has been unfocused
    /// </summary>
    public event EventHandler? MainWindowUnfocused;
}