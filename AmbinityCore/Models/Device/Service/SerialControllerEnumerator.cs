using System;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace AmbinityCore.Models.Device.Service;
/// <summary>
/// list all valid controller plugged in
/// </summary>
public static class SerialControllerEnumerator
{
    public static List<string> GetSerialPortByID(string vid, string pid)
    {
        var serialPorts = new List<string>();
        var currentOperatingSystem = OperatingSystem.IsMacOS();
        switch (currentOperatingSystem)
        {
            case true:
                serialPorts = GetMacSerialPortByID(vid, pid);
                break;
            case false:
                serialPorts = GetWindowsSerialPortByID(vid, pid);
                break;
            default:
        }
        return serialPorts;
    }
    private static List<string> GetWindowsSerialPortByID(String vid, String pid)
    {
        String pattern = String.Format("^VID_{0}.PID_{1}", vid, pid);
        Regex _rx = new Regex(pattern, RegexOptions.IgnoreCase);
        List<string> comports = new List<string>();
        RegistryKey rk1 = Registry.LocalMachine;
        RegistryKey rk2 = rk1.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum");
        foreach (String s3 in rk2.GetSubKeyNames())
        {
            RegistryKey rk3 = rk2.OpenSubKey(s3);
            foreach (String s in rk3.GetSubKeyNames())
            {
                if (_rx.Match(s).Success)
                {
                    RegistryKey rk4 = rk3.OpenSubKey(s);
                    foreach (String s2 in rk4.GetSubKeyNames())
                    {
                        RegistryKey rk5 = rk4.OpenSubKey(s2);
                        RegistryKey rk6 = rk5.OpenSubKey("Device Parameters");
                        string portName = (string)rk6.GetValue("PortName");
                        if (!String.IsNullOrEmpty(portName) && SerialPort.GetPortNames().Contains(portName))
                        {
                            comports.Add((string)rk6.GetValue("PortName"));
                        }
                    }
                }
            }
        }

        return comports;
    }

    private static List<string> GetMacSerialPortByID(String vid, String pid)
    {
        {
            var ports = new List<string>();
            var serialPorts = SerialPort.GetPortNames();
            for (int i = 0; i < serialPorts.Length; i++)
            {
                if (serialPorts[i].Contains("cu.usbmodem") || serialPorts[i].Contains("cu.wchusbserial"))
                {
                    ports.Add(serialPorts[i]);
                }
            }

            return ports;
        }
    }

    private static IntPtr CFString(string str)
    {
        return Marshal.StringToHGlobalAuto(str);

    }
}
