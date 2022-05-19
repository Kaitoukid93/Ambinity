using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace ConsoleApp1
{
    internal class Class1
    {
        private static SerialPort _serialPort = new SerialPort();
        private static  object _syncRoot = new object();
        private static byte[] requestCommand = { (byte)'d', (byte)'i', (byte)'r' };
        private static byte[] expectedValidHeader = { 15, 12, 93 };
        private static CancellationToken cancellationtoken;

        static void Main()
        {
            while(true)
            {
                Console.Write("Type something: ");
                _serialPort = new SerialPort("COM6", 1000000);
                _serialPort.DtrEnable = true;
                _serialPort.ReadTimeout = 5000;
                _serialPort.WriteTimeout = 1000;
                _serialPort.Open();


                ConsoleKeyInfo keyPress = Console.ReadKey(intercept: true);
                while (keyPress.Key != ConsoleKey.Enter)
                {
                    Console.Write(keyPress.KeyChar.ToString().ToUpper());

                    keyPress = Console.ReadKey(intercept: true);
                }
                Console.WriteLine();
                OnClick(cancellationtoken).Wait();
            }


          
        }
           
        
        static async Task OnClick( CancellationToken cancellationToken)
        {
            var jobTask = Task.Run(() => {
                // Organize critical sections around logical serial port operations somehow.
                lock (_syncRoot)
                {
                    return SomeCommand();
                }
            });
            if (jobTask != await Task.WhenAny(jobTask, Task.Delay(Timeout.Infinite, cancellationToken)))
            {
                // Timeout;
                return;
            }
            var response = await jobTask;
            Console.WriteLine("Name: " + response.Name);
            Console.WriteLine("ID: " + response.ID);
            Console.WriteLine("Firmware Version: " + response.FirmwareVersion);


            // Process response.
        }

        static Device SomeCommand()
        {
            // Assume serial port timeouts are set.
            byte[] id = new byte[256];
            byte[] name = new byte[256];
            byte[] fw = new byte[256];
            _serialPort.Write(requestCommand,0,3);

            int retryCount= 0;
            int offset = 0;
            int idLength = 0; // Expected response length of valid deviceID 
            int nameLength = 0; // Expected response length of valid deviceName 
            int fwLength = 0;
            Device newDevice = new Device();
            while (offset<3)
            {

            
                try
                {
                    byte header = (byte)_serialPort.ReadByte();
                    if (header == expectedValidHeader[offset])
                    {
                        offset++;
                    }
                }
                catch(TimeoutException)// retry until received valid header
                {
                    _serialPort.Write(requestCommand, 0, 3); 
                    retryCount++;
                    if (retryCount == 3)
                    {
                        Console.WriteLine("timeout waiting for respond on serialport " + _serialPort.PortName);
                    }
                    Console.WriteLine("no respond, retrying...");
                }
             

            }
            if(offset==3) //3 bytes header are valid
            {
              idLength = (byte)_serialPort.ReadByte();
                int count = idLength;
                 id = new byte[count];
                while (count > 0)
                {
                    var readCount = _serialPort.Read(id, 0, count);
                    offset += readCount;
                    count -= readCount;
                }


                newDevice.ID = BitConverter.ToString(id).Replace('-', ' ');
            }
            if (offset == 3+idLength) //3 bytes header are valid
            {
                nameLength = (byte)_serialPort.ReadByte();
                int count = nameLength;
                name = new byte[count];
                while (count > 0)
                {
                    var readCount = _serialPort.Read(name, 0, count);
                    offset += readCount;
                    count -= readCount;
                }
                newDevice.Name = Encoding.ASCII.GetString(name, 0, name.Length);
            }
            if (offset == 3 + idLength + nameLength) //3 bytes header are valid
            {
                fwLength = (byte)_serialPort.ReadByte();
                int count = fwLength;
                fw = new byte[count];
                while (count > 0)
                {
                    var readCount = _serialPort.Read(fw, 0, count);
                    offset += readCount;
                    count -= readCount;
                }
                newDevice.FirmwareVersion = Encoding.ASCII.GetString(fw, 0, fw.Length);
            }
            _serialPort.Close();
            _serialPort.Dispose();
            return newDevice;
           
        }
            
            
        }
    }
