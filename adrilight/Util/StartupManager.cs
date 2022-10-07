using Microsoft.Win32;
using System;
using System.IO;
using System.Security.Principal;
using System.Windows;

public class StartUpManager
{
    private const string ApplicationName = "adrilight";

    public static void AddApplicationToCurrentUserStartup()
    {
        try
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                var location = System.Reflection.Assembly.GetEntryAssembly().Location;
                string folder = Path.GetDirectoryName(location);
                key.SetValue(ApplicationName, folder + "\\" + "adrilight.exe");
            }
        }
        catch(Exception ex)
        {
            HandyControl.Controls.MessageBox.Show(" Không có quyền tự khởi động, app sẽ không khởi động cùng windows!!", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
       
    }

    public static void AddApplicationToAllUserStartup()
    {
        try
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                var location = System.Reflection.Assembly.GetEntryAssembly().Location;
                string folder = Path.GetDirectoryName(location);
                key.SetValue(ApplicationName, folder + "\\" + "adrilight.exe");
            }
        }
        catch (Exception ex)
        {
            HandyControl.Controls.MessageBox.Show(" Không có quyền tự khởi động, app sẽ không khởi động cùng windows!!", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
        }


    }

    public static void RemoveApplicationFromCurrentUserStartup()
    {
        try
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.DeleteValue(ApplicationName, false);
            }
        }
        catch (Exception ex)
        {
            HandyControl.Controls.MessageBox.Show(" Không có quyền tự khởi động, app sẽ không khởi động cùng windows!!", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

    }

    public static void RemoveApplicationFromAllUserStartup()
    {
        try
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.DeleteValue(ApplicationName, false);
            }
        }
        catch (Exception ex)
        {
            HandyControl.Controls.MessageBox.Show(" Không có quyền tự khởi động, app sẽ không khởi động cùng windows!!", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    public static bool IsUserAdministrator()
    {
        //bool value to hold our return value
        bool isAdmin;
        try
        {
            //get the currently logged in user
            WindowsIdentity user = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(user);
            isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch (UnauthorizedAccessException)
        {
            isAdmin = false;
        }
        catch (Exception)
        {
            isAdmin = false;
        }
        return isAdmin;
    }
}