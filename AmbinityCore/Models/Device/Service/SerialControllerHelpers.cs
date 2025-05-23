using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using AmbinityCore.Models.Device.Controller;
using Avalonia.Media;
using Serilog;

namespace AmbinityCore.Models.Device.Service;

public class SerialControllerHelpers
{
    private static byte[] requestCommand = { (byte)'d', (byte)'i', (byte)'r', (byte)'\n' };
    private static byte[] sendCommand = { (byte)'h', (byte)'s', (byte)'d' };
    private static byte[] expectedValidHeader = { 15, 12, 93 };

    public SerialControllerHelpers()
    {
    }

    private byte[] GetSettingOutputStream(SerialController controller)
    {
        var ledSettings = controller.LedController.HardwareSettings;
        var fanSettings = controller.FanController?.HardwareSettings;
        var outputStream = new byte[48];
        Buffer.BlockCopy(sendCommand, 0, outputStream, 0, sendCommand.Length);
        int counter = sendCommand.Length;
        outputStream[counter++] = (byte)(ledSettings.HWL_enable == true ? 1 : 0);
        outputStream[counter++] = (byte)(ledSettings.StatusLEDEnable == true ? 1 : 0);
        outputStream[counter++] = ledSettings.HWL_returnafter;
        outputStream[counter++] = ledSettings.HWL_effectMode;
        outputStream[counter++] = ledSettings.HWL_effectSpeed;
        outputStream[counter++] = ledSettings.HWL_brightness;
        outputStream[counter++] = ledSettings.HWL_singleColor.R;
        outputStream[counter++] = ledSettings.HWL_singleColor.G;
        outputStream[counter++] = ledSettings.HWL_singleColor.B;
        for (int i = 0; i < 8; i++)
        {
            outputStream[counter++] = ledSettings.HWL_palette[i].R;
            outputStream[counter++] = ledSettings.HWL_palette[i].G;
            outputStream[counter++] = ledSettings.HWL_palette[i].B;
        }

        outputStream[counter++] = ledSettings.HWL_effectIntensity;
        if (fanSettings != null)
            outputStream[counter++] = fanSettings.HW_FanSpeed;
        else
        {
            outputStream[counter++] = 0;
        }
        outputStream[counter++] = ledSettings.HWL_MaxLEDPerOutput;
        return outputStream;
    }

    private byte[] GetEEPRomDataOutputStream()
    {
        var outputStream = new byte[48];
        Buffer.BlockCopy(sendCommand, 0, outputStream, 0, sendCommand.Length);
        outputStream[3] = 255;
        return outputStream;
    }

    private bool IsFirmwareValid(SerialController controller)
    {
        if (controller.HardwareType == HardwareTypeEnum.AmbinoBasic ||
            controller.HardwareType == HardwareTypeEnum.AmbinoEDGE ||
            controller.HardwareType == HardwareTypeEnum.AmbinoFanHub ||
            controller.HardwareType == HardwareTypeEnum.AmbinoHUBV3)
        {
            string fwversion = controller.FirmwareVersion;
            if (fwversion == "unknown" || fwversion == string.Empty || fwversion == null)
                fwversion = "1.0.0";
            var deviceFWVersion = new Version(fwversion);
            var requiredVersion = new Version();
            switch (controller.HardwareType)
            {
                case HardwareTypeEnum.AmbinoBasic:
                    requiredVersion = new Version("1.0.8");
                    break;
                case HardwareTypeEnum.AmbinoEDGE:
                    requiredVersion = new Version("1.0.5");
                    break;
                case HardwareTypeEnum.AmbinoFanHub:
                    requiredVersion = new Version("1.0.8");
                    break;
            }


            if (deviceFWVersion >= requiredVersion)
            {
                return true;
            }

            return false;
        }

        return false;
    }

    /// <summary>
    /// get hardware setting from serial device
    /// any data stream that attached to device has to be disposed
    /// </summary>
    /// <param name="skipDeviceType"></param>
    /// <param name="hardware"></param>
    /// <returns></returns>
    public async Task<bool> GetHardwareSettings(bool skipDeviceType, SerialController controller)
    {
        Thread.Sleep(500);
        if (!skipDeviceType)
        {
            string deviceName = null;
            string deviceID = null;
            string deviceFirmware = null;
            string deviceHardware = null;
            int deviceHWL = 0;
            HardwareTypeEnum hardwareType = HardwareTypeEnum.Unknown;
            var result = RefreshDeviceInfo(controller.SerialPort, out deviceName, out deviceID, out deviceFirmware,
                out deviceHardware, out deviceHWL, out hardwareType);

            if (!result)
            {
                return false;
            }
            controller.Name = deviceName;
            controller.FirmwareVersion = deviceFirmware;
            controller.HardwareVersion = deviceHardware;
            controller.SerialNumber = deviceID;
            controller.HWLVersion = deviceHWL;
            if (!IsFirmwareValid(controller))
            {
                return false;
            }


            if (controller.HWLVersion < 1)
            {
                //request firmware update and hide device settings
                return false;
            }
        }

        if (!SerialPort.GetPortNames().Contains(controller.SerialPort))
            return false;
        var _serialPort = new SerialPort(controller.SerialPort, 1000000);
        _serialPort.DtrEnable = true;
        _serialPort.ReadTimeout = 5000;
        _serialPort.WriteTimeout = 1000;
        try
        {
            _serialPort.Open();
        }
        catch (UnauthorizedAccessException)
        {
            return await Task.FromResult(false);
        }
        catch (System.IO.IOException ex)
        {
            return await Task.FromResult(false);
        }

        var outputStream = GetEEPRomDataOutputStream();
        _serialPort.Write(outputStream, 0, outputStream.Length);
        _serialPort.WriteLine("\r\n");
        int retryCount = 0;
        int offset = 0;
        //searching for header
        while (offset < 3)
        {
            try
            {
                byte header = (byte)_serialPort.ReadByte();
                if (header == expectedValidHeader[offset])
                {
                    offset++;
                }
            }
            catch (TimeoutException) // retry until received valid header
            {
                _serialPort.Write(outputStream, 0, outputStream.Length);
                retryCount++;
                if (retryCount == 3)
                {
                    Log.Warning("timeout waiting for respond on serialport " + _serialPort.PortName);
                    _serialPort.Close();
                    _serialPort.Dispose();
                    return await Task.FromResult(false);
                }

                Debug.WriteLine("no respond, retrying...");
            }
        }

        //3 bytes header are valid continue to read next 45 byte of data
        if (offset == 3)
        {
            try
            {
                ReadDeviceEEPROM(_serialPort, controller);
                //discard buffer
                _serialPort.DiscardInBuffer();
            }
            catch (TimeoutException ex)
            {
                //discard buffer
                _serialPort.DiscardInBuffer();
                _serialPort.Close();
                _serialPort.Dispose();
                return await Task.FromResult(true);
            }
        }

        //discard buffer
        _serialPort.DiscardInBuffer();
        _serialPort.Close();
        _serialPort.Dispose();
        return await Task.FromResult(true);
    }

    public bool RefreshDeviceInfo(string comPort,
        out string deviceName,
        out string deviceID,
        out string deviceFirmware,
        out string deviceHardware,
        out int deviceHWL,
        out HardwareTypeEnum hardwareType)
    {
        deviceName = null;
        deviceID = null;
        deviceFirmware = null;
        deviceHardware = null;
        deviceHWL = 0;
        hardwareType = HardwareTypeEnum.Unknown;

        byte[] id = null, name = null, fw = null, hw = null;
        int idLength = 0, nameLength = 0, fwLength = 0, hwLength = 0;
        int offset = 0;
        var expectedHeader = expectedValidHeader;
        var _serialPort = new SerialPort(comPort, 1000000)
        {
            DtrEnable = true,
            ReadTimeout = 5000,
            WriteTimeout = 1000
        };
        try
        {
            _serialPort.Open();
            // Write request info command
            _serialPort.Write(requestCommand, 0, requestCommand.Length);
            int retryCount = 0;
            // Read 3-byte header
            while (offset < 3)
            {
                try
                {
                    byte header = (byte)_serialPort.ReadByte();
                    if (header == expectedHeader[offset])
                        offset++;
                }
                catch (TimeoutException)
                {
                    _serialPort.Write(requestCommand, 0, requestCommand.Length);
                    retryCount++;
                    if (retryCount >= 3)
                    {
                        Log.Error($"Timeout waiting for response on serialport {_serialPort.PortName}");
                        _serialPort.Close();
                        _serialPort.Dispose();
                        return false;
                    }
                }
            }
            if (offset != 3)
            {
                _serialPort.Close();
                _serialPort.Dispose();
                return false;
            }
            // Read deviceID
            idLength = _serialPort.ReadByte();
            id = new byte[idLength];
            _serialPort.Read(id, 0, idLength);
            deviceID = BitConverter.ToString(id).Replace("-", " ");
            // Read deviceName
            nameLength = _serialPort.ReadByte();
            name = new byte[nameLength];
            _serialPort.Read(name, 0, nameLength);
            deviceName = Encoding.ASCII.GetString(name, 0, name.Length);
            // Read deviceFirmware
            fwLength = _serialPort.ReadByte();
            fw = new byte[fwLength];
            _serialPort.Read(fw, 0, fwLength);
            deviceFirmware = Encoding.ASCII.GetString(fw, 0, fw.Length);
            // Read deviceHardware
            hwLength = _serialPort.ReadByte();
            hw = new byte[hwLength];
            _serialPort.Read(hw, 0, hwLength);
            deviceHardware = Encoding.ASCII.GetString(hw, 0, hw.Length);
            // Parse hardwareType
            if (!string.IsNullOrEmpty(deviceHardware) && deviceHardware.Length >= 2)
            {
                switch (deviceHardware.Substring(0, 2))
                {
                    case "AF": hardwareType = HardwareTypeEnum.AmbinoFanHub; break;
                    case "AH": hardwareType = HardwareTypeEnum.AmbinoHUBV3; break;
                    case "AB": hardwareType = HardwareTypeEnum.AmbinoBasic; break;
                    case "AE": hardwareType = HardwareTypeEnum.AmbinoEDGE; break;
                    default: hardwareType = HardwareTypeEnum.Unknown; break;
                }
            }
            // Read deviceHWL
            try
            {
                deviceHWL = _serialPort.ReadByte();
            }
            catch (TimeoutException)
            {
                Log.Information("Unknown Hardware Lighting Version");
            }
            _serialPort.Close();
            _serialPort.Dispose();
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
            try { _serialPort.Close(); _serialPort.Dispose(); } catch { }
            return false;
        }
    }

    public void ReadDeviceEEPROM(SerialPort _serialPort, SerialController controller)
    {
        /// Hardware Lighting Protocol version 1
        //+----------+---------------------+------------+---------------+
        //| Position | Name                | Size(byte) | Default Value |
        //+----------+---------------------+------------+---------------+
        //| 0        | HWL_enable          | 1          | 1             |
        //| 1        | StatusLEDEnable     | 1          | 0             |
        //| 2        | HWL_returnafter     | 1          | 3             |
        //| 3        | HWL_effectMode      | 1          | 0             |
        //| 4        | HWL_effectSpeed     | 1          | 20            |
        //| 5        | HWL_brightness      | 1          | 80            |
        //| 6 - 8    | HWL_singlecolor     | 3          | 255,0,0       |
        //| 9-32     | HWL_palette         | 24         |               |
        //| 33       | HWL_effectIntensity | 1          | 16            |
        //| 34       | HWF_FanSpeed        |            | 127           |
        //| 35-44    | HWF_PerFanSpeed     | 10         | 127           |
        //+----------+---------------------+------------+---------------+

        var ledSettings = controller.LedController.HardwareSettings;
        var fanSettings = controller.FanController?.HardwareSettings;
        ledSettings.HWL_enable = _serialPort.ReadByte() == 1 ? true : false;
        Log.Information("HWL_enable: " + ledSettings.HWL_enable);
        ledSettings.StatusLEDEnable = _serialPort.ReadByte() == 1 ? true : false;
        Log.Information("StatusLEDEnable: " + ledSettings.StatusLEDEnable);
        ledSettings.HWL_returnafter = (byte)_serialPort.ReadByte();
        Log.Information("HWL_returnafter: " + ledSettings.HWL_returnafter);
        ledSettings.HWL_effectMode = (byte)_serialPort.ReadByte();
        Log.Information("HWL_effectMode: " + ledSettings.HWL_effectMode);
        ledSettings.HWL_effectSpeed = (byte)_serialPort.ReadByte();
        Log.Information("HWL_effectSpeed: " + ledSettings.HWL_effectSpeed);
        ledSettings.HWL_brightness = (byte)_serialPort.ReadByte();
        Log.Information("HWL_brightness: " + ledSettings.HWL_brightness);
        ledSettings.HWL_singleColor = Color.FromRgb((byte)_serialPort.ReadByte(), (byte)_serialPort.ReadByte(),
            (byte)_serialPort.ReadByte());
        Log.Information("HWL_singleColor: " + ledSettings.HWL_singleColor);
        if (ledSettings.HWL_palette == null)
        {
            ledSettings.HWL_palette = new Color[8];
        }

        for (int i = 0; i < 8; i++)
        {
            ledSettings.HWL_palette[i] = Color.FromRgb((byte)_serialPort.ReadByte(), (byte)_serialPort.ReadByte(),
                (byte)_serialPort.ReadByte());
            Log.Information("HWL_palette " + i + ": " + ledSettings.HWL_palette[i]);
        }

        ledSettings.HWL_effectIntensity = (byte)_serialPort.ReadByte();
        Log.Information("HWL_effectIntensity: " + ledSettings.HWL_effectIntensity);
        if (fanSettings != null)
        {
            var noSignalFanSpeed = _serialPort.ReadByte();
            fanSettings.HW_FanSpeed = noSignalFanSpeed < 20 ? (byte)20 : (byte)noSignalFanSpeed;
            Log.Information("NoSignalFanSpeed: " + noSignalFanSpeed);
        }

        ledSettings.HWL_MaxLEDPerOutput = (byte)_serialPort.ReadByte();
        Log.Information("HWL_MaxLEDPerOutput: " + ledSettings.HWL_MaxLEDPerOutput);
    }

    public async Task<bool> SendHardwareSettings(SerialController controller)
    {
        var _serialPort = new SerialPort(controller.SerialPort, 1000000);
        _serialPort.DtrEnable = true;
        _serialPort.ReadTimeout = 5000;
        _serialPort.WriteTimeout = 1000;
        try
        {
            _serialPort.Open();
        }
        catch (UnauthorizedAccessException)
        {
            return await Task.FromResult(false);
        }

        var outputStream = GetSettingOutputStream(controller);
        // vm.Value = 25;
        await Task.Delay(1000);
        _serialPort.Write(outputStream, 0, outputStream.Length);
        _serialPort.WriteLine("\r\n");
        int retryCount = 0;
        int offset = 0;
        while (offset < 3)
        {
            try
            {
                byte header = (byte)_serialPort.ReadByte();
                if (header == expectedValidHeader[offset])
                {
                    offset++;
                }
            }
            catch (TimeoutException) // retry until received valid header
            {
                _serialPort.Write(outputStream, 0, outputStream.Length);
                retryCount++;
                if (retryCount == 3)
                {
                    Console.WriteLine("timeout waiting for respond on serialport " + _serialPort.PortName);
                    _serialPort.Close();
                    _serialPort.Dispose();
                    return await Task.FromResult(false);
                }

                Debug.WriteLine("no respond, retrying...");
            }
        }

        if (offset == 3) //3 bytes header are valid continue to read next 13 byte of data
        {
            ReadDeviceEEPROM(_serialPort, controller);
            //discard buffer
            _serialPort.DiscardInBuffer();
        }

        _serialPort.Close();
        _serialPort.Dispose();
        for (int i = 0; i < 10; i++)
        {
            // vm.Value += 10;
            //   if (vm.Value > 100)
            //        vm.Value = 100;
            await Task.Delay(200);
        }

        //  ShowUpdateSuccessMessage(vm, device);
        return await Task.FromResult(true);
    }

}
