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

            string hexString = BitConverter.ToString(response);
            Console.WriteLine("ID: " + hexString);
            
            // Process response.
        }

        static byte[] SomeCommand()
        {
            // Assume serial port timeouts are set.
            byte[] info = new byte[256];
            _serialPort.Write(requestCommand,0,3);

            int retryCount= 0;
            int offset = 0;
            int infoLength = 0; // Expected response length of valid header
            
            while(offset<3)
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
              infoLength = (byte)_serialPort.ReadByte();
                int count = infoLength;
                 info = new byte[count];
                while (count > 0)
                {
                    var readCount = _serialPort.Read(info, 0, count);
                    offset += readCount;
                    count -= readCount;
                }

            }
            return info;
            _serialPort.Close();
            _serialPort.Dispose();
        }
            
            
        }
    }

