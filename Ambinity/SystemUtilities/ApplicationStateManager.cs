using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using AmbinityCore;
using AmbinityCore.Events;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using Serilog;

namespace Ambinity.SystemUtilities;

/// <summary>
/// code taken from Artemis
/// </summary>
public class ApplicationStateManager
{
    private const int SM_SHUTTINGDOWN = 0x2000;
    
    public ApplicationStateManager(string[] startupArguments)
    {
 
        StartupArguments = startupArguments;
        IsElevated = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        AmbinityCore.Utils.Utilities.ShutdownRequested += UtilitiesOnShutdownRequested;
        AmbinityCore.Utils.Utilities.RestartRequested += UtilitiesOnRestartRequested;
        AmbinityCore.Utils.Utilities.UpdateRequested += UtilitiesOnUpdateRequested;

        // On Windows shutdown dispose the IOC container just so device providers get a chance to clean up
        if (Application.Current?.ApplicationLifetime is IControlledApplicationLifetime controlledApplicationLifetime)
            controlledApplicationLifetime.Exit += ControlledApplicationLifetimeOnExit;

        // Inform the Core about elevation status
       // container.Resolve<ICoreService>().IsElevated = IsElevated;
    }

    public string[] StartupArguments { get; }

    public bool IsElevated { get; }

    private void UtilitiesOnRestartRequested(object? sender, RestartEventArgs e)
    {
        List<string> argsList = new();
        argsList.AddRange(StartupArguments);
        if (e.ExtraArgs != null)
            argsList.AddRange(e.ExtraArgs.Except(argsList));
        string args = argsList.Any() ? "-ArgumentList " + string.Join(',', argsList) : "";
        string command =
            $"-Command \"& {{Start-Sleep -Milliseconds {(int) e.Delay.TotalMilliseconds}; " +
            "(Get-Process 'Ambinity').kill(); " +
            $"Start-Process -FilePath '{Constants.ExecutablePath}' -WorkingDirectory '{Constants.ApplicationFolder}' {args}}}\"";
        // Elevated always runs with RunAs
        if (e.Elevate)
        {
            ProcessStartInfo info = new()
            {
                Arguments = command.Replace("}\"", " -Verb RunAs}\""),
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "PowerShell.exe"
            };
            Process.Start(info);
        }
        // Non-elevated runs regularly if currently not elevated
        else if (!IsElevated)
        {
            ProcessStartInfo info = new()
            {
                Arguments = command,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "PowerShell.exe"
            };
            Process.Start(info);
        }
        // Non-elevated runs via a utility method is currently elevated (de-elevating is hacky)
        // Ambinity is not using it right now since the app always need to be elevated
        // else
        // {
        //     string powerShell = Path.Combine(Environment.SystemDirectory, "WindowsPowerShell", "v1.0", "powershell.exe");
        //     ProcessUtilities.RunAsDesktopUser(powerShell, command, true);
        // }

        // Lets try a graceful shutdown, PowerShell will kill if needed
        if (Application.Current?.ApplicationLifetime is IControlledApplicationLifetime controlledApplicationLifetime)
            Dispatcher.UIThread.Post(() => controlledApplicationLifetime.Shutdown());
    }

    private void UtilitiesOnUpdateRequested(object? sender, UpdateEventArgs e)
    {
        List<string> argsList = new(StartupArguments);
        if (e.Silent && !argsList.Contains("--minimized"))
            argsList.Add("--minimized");

        // Retain startup arguments after update by providing them to the script
        string script = Path.Combine(Constants.UpdatingFolder, "installing", "scripts", "update.ps1");
        string source = $"-sourceDirectory \"'{Path.Combine(Constants.UpdatingFolder, "installing")}'\"";
        string destination = $"-destinationDirectory \"'{Constants.ApplicationFolder}'\"";
        string args = argsList.Any() ? $"-artemisArgs \"'{string.Join(' ', argsList)}'\"" : "";

        RunScriptWithOutputFile(script, $"{source} {destination} {args}", Path.Combine(Constants.AppDataFolder, "update-log.txt"));

        // Lets try a graceful shutdown, PowerShell will kill if needed
        if (Application.Current?.ApplicationLifetime is IControlledApplicationLifetime controlledApplicationLifetime)
            Dispatcher.UIThread.Post(() => controlledApplicationLifetime.Shutdown());
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
        // Don't run a forced shutdown if Windows itself is shutting down, the new PowerShell process will fail
        if (GetSystemMetrics(SM_SHUTTINGDOWN) != 0 || StartupArguments.Contains("--disable-forced-shutdown"))
            return;

        ProcessStartInfo info = new()
        {
            Arguments = "-Command \"& {Start-Sleep -s 8; (Get-Process -Id " + Process.GetCurrentProcess().Id + ").kill()}",
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            FileName = "PowerShell.exe"
        };
        Process.Start(info);
    }

    private void RunScriptWithOutputFile(string script, string arguments, string outputFile)
    {
        // Use > for files that are bigger than 200kb to start fresh, otherwise use >> to append
        string redirectSymbol = File.Exists(outputFile) && new FileInfo(outputFile).Length > 200000 ? ">" : ">>";
        ProcessStartInfo info = new()
        {
            Arguments = $"PowerShell -ExecutionPolicy Bypass -File \"{script}\" {arguments} {redirectSymbol} \"{outputFile}\"",
            FileName = "PowerShell.exe",
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
        };
        Process.Start(info);
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);
}