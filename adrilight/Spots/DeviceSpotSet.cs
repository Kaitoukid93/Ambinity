﻿using adrilight.DesktopDuplication;
using adrilight.Extensions;
using adrilight.Spots;
using BO;
using NLog;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace adrilight
{
    internal sealed class DeviceSpotSet : IDeviceSpotSet
    {
        private ILogger _log = LogManager.GetCurrentClassLogger();
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string JsonLEDSetupFileNameAndPath => Path.Combine(JsonPath, "adrilight-LEDSetups.json");
        public DeviceSpotSet(IOutputSettings outputSettings, IGeneralSettings generalSettings)
        {
            OutputSettings = outputSettings ?? throw new ArgumentNullException(nameof(outputSettings));
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));

            OutputSettings.PropertyChanged += (_, e) => DecideRefresh(e.PropertyName);
            GeneralSettings.PropertyChanged += (_, e) => DecideRefresh(e.PropertyName);
            Refresh();

            _log.Info($"SpotSet created.");
        }

        private void DecideRefresh(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(OutputSettings.OutputNumLEDX):
                case nameof(OutputSettings.OutputNumLEDY):
                case nameof(OutputSettings.OutputNumLED):
                case nameof(OutputSettings.OutputPixelWidth):
                case nameof(OutputSettings.OutputPixelHeight):
                //... there are more to come
                    Refresh();
                    break;
            }
        }


        public ILEDSetup LEDSetup { get; set; }



        public object Lock { get; } = new object();


        /// <summary>
        /// returns the number of leds
        /// </summary>






    

    


        private IOutputSettings OutputSettings { get; }
        private IGeneralSettings GeneralSettings { get; }
        private void Refresh()
        {
            

            lock (Lock)
            {
                OutputSettings.OutputLEDSetup = BuildLEDSetup(OutputSettings, GeneralSettings);

            }

        }

        internal ILEDSetup BuildLEDSetup(IOutputSettings outputSettings, IGeneralSettings generalSettings) // general settings is for compare each device setting
        {

            var availableLEDSetups = LoadSetupIfExist();
            int matrixWidth = outputSettings.OutputNumLEDX;
            int matrixHeight = outputSettings.OutputNumLEDY;
            int numLED = outputSettings.OutputNumLED;
            string name = outputSettings.OutputName;
            string owner = "Ambino";
            string description = "Default LED Setup for Ambino Basic Rev 2";
            string type = "ABRev2";
            int setupID = outputSettings.OutputID;
            int rectWidth = outputSettings.OutputPixelWidth;
            int rectHeight = outputSettings.OutputPixelHeight;
            
            IDeviceSpot[] spots = new DeviceSpot[numLED];
            List<IDeviceSpot> reorderedSpots = new List<IDeviceSpot>();

            //Create default spot
            var availableSpots = BuildMatrix(rectWidth, rectHeight, matrixWidth, matrixHeight);
            int counter = 0;
            switch(outputSettings.OutputType)
            {
                case "Frame":
                    for (var i = 0; i < matrixHeight; i++) // bottom right ( default ambino basic start point) go up to top right
                    {
                        var spot = availableSpots[availableSpots.Length - 1 - matrixWidth * i];
                        spot.IsActivated = true;
                        spot.id = counter++;
                        reorderedSpots.Add(spot);
                    }
                    for (var i = 0; i < matrixWidth - 1; i++) // top right go left to top left
                    {
                        var spot = availableSpots[matrixWidth - 2 - i];
                        spot.IsActivated = true;
                        spot.id = counter++;
                        reorderedSpots.Add(spot);
                    }
                    for (var i = 0; i < matrixHeight - 1; i++) // top left go down to bottom left
                    {
                        var spot = availableSpots[matrixWidth * (i + 1)];
                        spot.IsActivated = true;
                        spot.id = counter++;
                        reorderedSpots.Add(spot);
                    }
                    for (var i = 0; i < matrixWidth - 2; i++) // top left go down to bottom left
                    {
                        var spot = availableSpots[matrixWidth * (matrixHeight - 1) + i + 1];
                        spot.IsActivated = true;
                        spot.id = counter++;
                        reorderedSpots.Add(spot);

                    }
                    break;
                case "Keyboard":
                    foreach(var spot in availableSpots)
                    {
                        spot.IsActivated = true;
                        reorderedSpots.Add(spot);
                    }
                    break;
                case "ABEDGE":
                    foreach (var spot in availableSpots)
                    {
                        if(spot.YIndex == 0)
                        spot.IsActivated = true;
                        reorderedSpots.Add(spot);
                    }
                    break;


            }
           
            counter = 0;

            IDeviceSpot[] reorderedActiveSpots = new DeviceSpot[reorderedSpots.Count];
            
                foreach(var spot in reorderedSpots)
            {
                reorderedActiveSpots[counter++] = spot;
            }

            ILEDSetup ledSetup = new LEDSetup(name, owner, type, description, reorderedActiveSpots, matrixWidth, matrixHeight, setupID);
           
            
            //if (availableLEDSetups != null)
            //{
            //    foreach (var ledsetup in availableLEDSetups)
            //    {
            //        if (ledsetup.SetupID == outputSettings.OutputUniqueID)//found match
            //            ledSetup = ledsetup;

            //    }
            //}
         


            return ledSetup;
        }


        public List<LEDSetup> LoadSetupIfExist()
        {
            if (!File.Exists(JsonLEDSetupFileNameAndPath)) return null;

            var json = File.ReadAllText(JsonLEDSetupFileNameAndPath);

            var ledSetups = JsonConvert.DeserializeObject<List<LEDSetup>>(json);

            return ledSetups;
        }

        private IDeviceSpot[] BuildMatrix(int rectwidth, int rectheight, int spotsX, int spotsY)
        {
            int spacing = 3;
            IDeviceSpot[] spotSet = new DeviceSpot[spotsX*spotsY];
            var compareWidth = (rectwidth-(spacing*(spotsX+1)))/ spotsX;
            var compareHeight = (rectheight-(spacing*(spotsY+1)))/ spotsY;
            var spotSize = Math.Min(compareWidth, compareHeight);
            

            //var startPoint = (Math.Max(rectheight,rectwidth) - spotSize * Math.Min(spotsX, spotsY))/2;
            var counter = 0;




            for (var j = 0; j < spotsY; j++)
            {
                for (var i = 0; i < spotsX; i++)
                {
                    var x = spacing*i+(rectwidth -  (spotsX * spotSize) -spacing*(spotsX-1))/2 + i*spotSize;
                    var y = spacing*j+(rectheight - (spotsY * spotSize)-spacing*(spotsY-1))/2 + j*spotSize;
                    var index = counter;

                    spotSet[index] = new DeviceSpot(i,j,x, y, spotSize, spotSize, 0, 0, 0, 0,index, index, index, index, false);
                    counter++;

                }
            }

            return spotSet;

        }




    }
}
