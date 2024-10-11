using System.Diagnostics;
using AmbinityCore.Events;

namespace AmbinityCore.Utils;

/// <summary>
/// provide some method for controlling application, some part of the code taken from Artemis
/// </summary>
public static class Utilities
{
    /// <summary>
    ///     Attempts to gracefully shut down the application with a delayed kill to ensure the application shut down
    ///     <para>
    ///         This is required because not all SDKs shut down properly, it is too unpredictable to just assume we can
    ///         gracefully shut down
    ///     </para>
    /// </summary>
    private static bool _shuttingDown;
    public static void Shutdown()
    {
        if (_shuttingDown)
            return;
        
        // Request a graceful shutdown, whatever UI we're running can pick this up
        _shuttingDown = true;
        OnShutdownRequested();
    }

    /// <summary>
    ///     Restarts the application
    /// </summary>
    /// <param name="elevate">Whether the application should be restarted with elevated permissions</param>
    /// <param name="delay">Delay in seconds before killing process and restarting </param>
    /// <param name="extraArgs">A list of extra arguments to pass to Artemis when restarting</param>
    public static void Restart(bool elevate, TimeSpan delay, params string[] extraArgs)
    {
        if (_shuttingDown)
            return;

        if (!OperatingSystem.IsWindows() && elevate)
            throw new Exception("Elevation on non-Windows platforms is not supported.");

        _shuttingDown = true;
        OnRestartRequested(new RestartEventArgs(elevate, delay, extraArgs.ToList()));
    }

    /// <summary>
    ///     Applies a pending update
    /// </summary>
    /// <param name="silent">A boolean indicating whether to silently update or not.</param>
    public static void ApplyUpdate(bool silent)
    {
        OnUpdateRequested(new UpdateEventArgs(silent));
    }

    /// <summary>
    ///     Opens the provided URL in the default web browser
    /// </summary>
    /// <param name="url">The URL to open</param>
    /// <returns>The process created to open the URL</returns>
    public static Process? OpenUrl(string url)
    {
        ProcessStartInfo processInfo = new()
        {
            FileName = url,
            UseShellExecute = true
        };
        return Process.Start(processInfo);
    }
    /// <summary>
    ///     Occurs when the core has requested an application shutdown
    /// </summary>
    public static event EventHandler? ShutdownRequested;

    /// <summary>
    ///     Occurs when the core has requested an application restart
    /// </summary>
    public static event EventHandler<RestartEventArgs>? RestartRequested;

    /// <summary>
    ///     Occurs when the core has requested a pending application update to be applied
    /// </summary>
    public static event EventHandler<UpdateEventArgs>? UpdateRequested;
    /// <summary>
    ///     Gets the current application location
    /// </summary>
    /// <returns></returns>
    internal static string GetCurrentLocation()
    {
        return Process.GetCurrentProcess().MainModule!.FileName!;
    }

    private static void OnRestartRequested(RestartEventArgs e)
    {
        RestartRequested?.Invoke(null, e);
    }

    private static void OnShutdownRequested()
    {
        ShutdownRequested?.Invoke(null, EventArgs.Empty);
    }

    private static void OnUpdateRequested(UpdateEventArgs e)
    {
        UpdateRequested?.Invoke(null, e);
    }
    
}