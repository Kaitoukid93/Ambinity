using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Ambinity.Events;
using Ambinity.Utils;
using AmbinityCore.CapturingService;
using AmbinityCore.Events;
using AmbinityCore.Models.Device.Service;
using AmbinityCore.Models.Profile;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Serilog;

namespace Ambinity.MacOs.SystemUtilities;

/// <summary>
/// code taken from Artemis, todo implement a core services that hold all the services
/// </summary>
public class ApplicationStateManager
{
    private LightingProfileDecoder _decoder;
    private CapturingServiceProvider _capturingServiceProvider;
    private SerialControllerDiscoveryService _serialDiscoveryService;

    public ApplicationStateManager(CapturingServiceProvider capturingServiceProvider,
    LightingProfileDecoder decoder, SerialControllerDiscoveryService serialDiscoveryService)
    {


        _decoder = decoder;
        _capturingServiceProvider = capturingServiceProvider;
        _serialDiscoveryService = serialDiscoveryService;
        Utilities.ShutdownRequested += UtilitiesOnShutdownRequested;
        Utilities.RestartRequested += UtilitiesOnRestartRequested;
        Utilities.UpdateRequested += UtilitiesOnUpdateRequested;
        Utilities.SleepRequested += UtilitiesOnSleepRequested;
        Utilities.WakeupRequested += UtilitiesOnWakeupRequested;

        // On Windows shutdown dispose the IOC container just so device providers get a chance to clean up
        if (Application.Current?.ApplicationLifetime is IControlledApplicationLifetime controlledApplicationLifetime)
            controlledApplicationLifetime.Exit += ControlledApplicationLifetimeOnExit;
        //
        // Inform the Core about elevation status
        // container.Resolve<ICoreService>().IsElevated = IsElevated;
    }

    private async void UtilitiesOnSleepRequested(object? sender, EventArgs e)
    {
        //Stop current playing profile
        //stop screencapturingservice
        //stop audiocapturingservice
        //stop serialdiscoveryservice
        //stop hwmonitorservice
        Log.Information("Going to sleep, disposing capturing services");
        await _decoder.Stop();
        await _capturingServiceProvider.Dispose();
        _serialDiscoveryService.Hold();

    }
    private async void UtilitiesOnWakeupRequested(object? sender, EventArgs e)
    {
        Log.Information("Waking up from sleep, re-initializing capturing services");
        await _capturingServiceProvider.Init();
        _decoder.Init();
        await _serialDiscoveryService.Resume(1);
        //start screencapturingservice
        //start audiocapturingservice
        //start serialdiscoveryservice
        //start hwmonitorservice
        //start current playing profile
    }

    private void UtilitiesOnRestartRequested(object? sender, RestartEventArgs e)
    {
        // Get the path to the current executable
        var exePath = Process.GetCurrentProcess().MainModule?.FileName;
        if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
        {
            Process.Start(exePath);
        }

        // Gracefully shutdown the current application
        if (Application.Current?.ApplicationLifetime is IControlledApplicationLifetime controlledApplicationLifetime)
            Dispatcher.UIThread.Post(() => controlledApplicationLifetime.Shutdown());
    }

    private void UtilitiesOnUpdateRequested(object? sender, UpdateEventArgs e)
    {
        //todo implement MacOs update manager
    }

    private void ControlledApplicationLifetimeOnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        Log.Information("Application lifetime exiting, disposing container and friends");

        RunForcedShutdownIfEnabled();

        // Dispose plugins before disposing the IOC container because plugins might access services during dispose
        // dispose all engine and service, save data
    }

    private void UtilitiesOnShutdownRequested(object? sender, EventArgs e)
    {
        // Use PowerShell to kill the process after 8 sec just in case
        RunForcedShutdownIfEnabled();

        if (Application.Current?.ApplicationLifetime is IControlledApplicationLifetime controlledApplicationLifetime)
            Dispatcher.UIThread.Post(() => controlledApplicationLifetime.Shutdown());
    }

    private void RunForcedShutdownIfEnabled()
    {

    }
}
