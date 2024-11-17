using System;
using System.Security.Principal;
using AmbinityCore;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;

namespace Ambinity.SystemUtilities;

public static class StartUpManager
{
    private const string ApplicationName = "adrilight";

    public static void AddApplicationToTaskScheduler(string taskName, int delaySecond, bool update = false)
    {
        TaskService ts = new TaskService();
        if (ts.GetTask(taskName) != null && !update)
            return;
        TaskDefinition td = ts.NewTask();
        //td.Principal.RunLevel = TaskRunLevel.Highest;
        //td.Triggers.AddNew(TaskTriggerType.Logon);          
        string program_path = Constants.ExecutablePath; // you can have it dynamic
        LogonTrigger lg = new LogonTrigger { UserId = WindowsIdentity.GetCurrent().Name };
        lg.Delay = TimeSpan.FromSeconds(delaySecond);
        td.Triggers.Add(lg); //even of user choice giving an interface in win-form or wpf application
        td.Actions.Add(new ExecAction(program_path, null));
        ts.RootFolder.RegisterTaskDefinition(taskName, td);
    }

    public static void RemoveApplicationFromTaskScheduler(string taskName)
    {
        using TaskService ts = new TaskService();
        if (ts.GetTask(taskName) != null)
        {
            ts.RootFolder.DeleteTask(taskName);
        }
    }
    
}