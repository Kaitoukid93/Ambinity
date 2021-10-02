using adrilight.DesktopDuplication;
using adrilight.Extensions;
using adrilight.Spots;
using BO;
using NLog;
using System;
using System.Drawing;
using System.Windows.Forms;


namespace adrilight
{
    internal sealed class DeviceSpotSet : IDeviceSpotSet
    {
        private ILogger _log = LogManager.GetCurrentClassLogger();

        public DeviceSpotSet(IDeviceSettings deviceSettings, IGeneralSettings generalSettings)
        {
            DeviceSettings = deviceSettings ?? throw new ArgumentNullException(nameof(deviceSettings));
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));

            DeviceSettings.PropertyChanged += (_, e) => DecideRefresh(e.PropertyName);
            GeneralSettings.PropertyChanged += (_, e) => DecideRefresh(e.PropertyName);
            Refresh();

            _log.Info($"SpotSet created.");
        }
      
        private void DecideRefresh(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(DeviceSettings.SpotsX):
                case nameof(DeviceSettings.SpotsY):
                case nameof(GeneralSettings.ScreenSize):
                case nameof(DeviceSettings.SelectedDisplay):
                case nameof(DeviceSettings.SelectedEffect):
                case nameof(GeneralSettings.ScreenSizeSecondary):
                case nameof(GeneralSettings.ScreenSizeThird):
                case nameof(DeviceSettings.OffsetLed):
                case nameof(DeviceSettings.MirrorX):
                case nameof(DeviceSettings.MirrorY):
                case nameof(DeviceSettings.DeviceRectWidth):
                case nameof(DeviceSettings.DeviceRectHeight):
                case nameof(DeviceSettings.NumLED):




                    Refresh();
                    break;
            }
        }


        public IDeviceSpot[] Spots { get; set; }
       


        public object Lock { get; } = new object();


        /// <summary>
        /// returns the number of leds
        /// </summary>
        public int CountLeds(int spotsX, int spotsY)
        {
            if (spotsX <= 1 || spotsY <= 1)
            {
                //special case because it is not really a rectangle of lights but a single light or a line of lights
                return spotsX * spotsY;
            }

            //normal case
            return 2 * spotsX + 2 * spotsY - 4;
            //return spotsX * spotsY;
        }
        private int _iD;

        public int ID {
            get { return DeviceSettings.DeviceID; }
            set
            {
                _iD = value;
            }
        }

        private int _parrentLocation;

        public int ParrentLocation 
            { 
            get { return DeviceSettings.ParrentLocation; }
            set
            {
            _parrentLocation= value;
        }
        }
        private int _outputLocation;
        public int OutputLocation {
            get { return DeviceSettings.OutputLocation; }
            set
            {
                _outputLocation = value;
            }
        }

        private int _rGBOrder;
        public int RGBOrder {
            get { return DeviceSettings.RGBOrder; }
            set
            {
                _rGBOrder = value;
            }
        }


        private IDeviceSettings DeviceSettings { get; }
        private IGeneralSettings GeneralSettings { get; }
        private void Refresh()
        {
            lock (Lock)
            {
                Spots = BuildDeviceSpots(DeviceSettings, GeneralSettings);
                


            }
           // DeviceSettings.Spots = Spots;
        }

      


        [Obsolete]
        internal IDeviceSpot[] BuildSpots( IDeviceSettings userSettings, IGeneralSettings generalSettings) // general settings is for compare each device setting
        {
            //this section build spot according to user selection
            //in the future, user will select the rectangle on a matrix map then we take that rectangle cordinate
            // this kind of selection only available at gifxelation and screen capture mode
            // with others mode, we arrange as the default shape ( square for screen, round for fan, straight for desk led)

            var spotsX = userSettings.SpotsX;
            var spotsY = userSettings.SpotsY;
            var offset = 10;
            IDeviceSpot[] devicespots = new DeviceSpot[160]; //maximum number of spot
            if(DeviceSettings.SelectedEffect==0)
            {
                if(DeviceSettings.DeviceType=="ABRev1" || DeviceSettings.DeviceType=="ABRev2")
                {

                  
                if (DeviceSettings.SelectedDisplay == 0) // 
                {
                    if (spotsX != generalSettings.SpotsX || spotsY != generalSettings.SpotsY)// check if user input over kill the parrent's matrix that precreated
                    {
                        spotsX = generalSettings.SpotsX;
                        spotsY = generalSettings.SpotsY; // revert to default value of spotsX and spotsY
                        // MessageBox.Show("Can not create a matrix that greater than original matrix, please enter a smaller value!!!");
                        devicespots = new DeviceSpot[CountLeds(spotsX, spotsY)];
                    }
                    else

                    {
                        devicespots = new DeviceSpot[CountLeds(spotsX, spotsY)];
                    }
                }
                else if (DeviceSettings.SelectedDisplay == 1)
                {
                    if (spotsX != generalSettings.SpotsX2 || spotsY != generalSettings.SpotsY2)// check if user input over kill the parrent's matrix that precreated
                    {
                        spotsX = generalSettings.SpotsX2;
                        spotsY = generalSettings.SpotsY2; // revert to default value of spotsX and spotsY
                        // MessageBox.Show("Can not create a matrix that greater than original matrix, please enter a smaller value!!!");
                        devicespots = new DeviceSpot[CountLeds(spotsX, spotsY)];
                    }
                    else

                    {
                        devicespots = new DeviceSpot[CountLeds(spotsX, spotsY)];
                    }
                }
                else if (DeviceSettings.SelectedDisplay == 2)
                {
                    if (spotsX != generalSettings.SpotsX3 || spotsY != generalSettings.SpotsY3)// check if user input over kill the parrent's matrix that precreated
                    {
                        spotsX = generalSettings.SpotsX3;
                        spotsY = generalSettings.SpotsY3; // revert to default value of spotsX and spotsY
                        // MessageBox.Show("Can not create a matrix that greater than original matrix, please enter a smaller value!!!");
                        devicespots = new DeviceSpot[CountLeds(spotsX, spotsY)];
                    }
                    else

                    {
                        devicespots = new DeviceSpot[CountLeds(spotsX, spotsY)];
                    }
                }
                }
                else if(DeviceSettings.DeviceType=="ABEDGE")
                {
                    devicespots = new DeviceSpot[CountLeds(spotsX, spotsY)];
                }
                else if(DeviceSettings.DeviceType=="Strip")
                {
                    devicespots = new DeviceSpot[CountLeds(spotsX, spotsY)];
                }
                else if (DeviceSettings.DeviceType == "Square")
                {
                    if (DeviceSettings.SelectedDisplay == 0) // 
                    {
                        if (spotsX != generalSettings.SpotsX || spotsY != generalSettings.SpotsY)// check if user input over kill the parrent's matrix that precreated
                        {
                            spotsX = generalSettings.SpotsX;
                            spotsY = generalSettings.SpotsY; // revert to default value of spotsX and spotsY
                                                             // MessageBox.Show("Can not create a matrix that greater than original matrix, please enter a smaller value!!!");
                            devicespots = new DeviceSpot[CountLeds(spotsX, spotsY)];
                        }
                        else

                        {
                            devicespots = new DeviceSpot[CountLeds(spotsX, spotsY)];
                        }
                    }
                    else if (DeviceSettings.SelectedDisplay == 1)
                    {
                        if (spotsX != generalSettings.SpotsX2 || spotsY != generalSettings.SpotsY2)// check if user input over kill the parrent's matrix that precreated
                        {
                            spotsX = generalSettings.SpotsX2;
                            spotsY = generalSettings.SpotsY2; // revert to default value of spotsX and spotsY
                                                              // MessageBox.Show("Can not create a matrix that greater than original matrix, please enter a smaller value!!!");
                            devicespots = new DeviceSpot[CountLeds(spotsX, spotsY)];
                        }
                        else

                        {
                            devicespots = new DeviceSpot[CountLeds(spotsX, spotsY)];
                        }
                    }
                    else if (DeviceSettings.SelectedDisplay == 2)
                    {
                        if (spotsX != generalSettings.SpotsX3 || spotsY != generalSettings.SpotsY3)// check if user input over kill the parrent's matrix that precreated
                        {
                            spotsX = generalSettings.SpotsX3;
                            spotsY = generalSettings.SpotsY3; // revert to default value of spotsX and spotsY
                                                              // MessageBox.Show("Can not create a matrix that greater than original matrix, please enter a smaller value!!!");
                            devicespots = new DeviceSpot[CountLeds(spotsX, spotsY)];
                        }
                        else

                        {
                            devicespots = new DeviceSpot[CountLeds(spotsX, spotsY)];
                        }
                    }
                }
                else 
                {
                    devicespots = new DeviceSpot[CountLeds(spotsX, spotsY)];
                }
            }

            else
            {
                devicespots = new DeviceSpot[CountLeds(spotsX, spotsY)];

            }


            //just this is enough????
            // next, everytime a frame update, spotsetreader (which attached to every single device) will set the color of each device spots in devicespotset acording to index
            // SpotSet reader only service gifxelation and screen capture mode
            // the treeview is Desktopduplicator(x)-> desktopduplicatorReader(x)-> SpotSetReader(device)->Viewmodel(device)->SerialStream(device)
            var screenWidth = DeviceSettings.DeviceRectWidth;
            var screenHeight = DeviceSettings.DeviceRectHeight;
            if (userSettings.DeviceSize==0|| userSettings.DeviceSize == 1|| userSettings.DeviceSize == 3)
            {
                 screenWidth = DeviceSettings.DeviceRectWidth;
                 screenHeight = DeviceSettings.DeviceRectHeight;
            }
            else
            {
                screenWidth = DeviceSettings.DeviceRectWidth;
                screenHeight = DeviceSettings.DeviceRectHeight;
            }
            
            var scalingFactor = DesktopDuplicator.ScalingFactor;
            var borderDistanceX = userSettings.BorderDistanceX / scalingFactor;
            var borderDistanceY = userSettings.BorderDistanceY / scalingFactor;
            var spotWidth = screenWidth / userSettings.SpotsX;
            var spotHeight = spotWidth;

            var canvasSizeX = screenWidth - 2 * borderDistanceX;
            var canvasSizeY = screenHeight - 2 * borderDistanceY;


            var xResolution = spotsX > 1 ? (canvasSizeX - spotWidth) / (spotsX - 1) : 0;
            var xRemainingOffset = spotsX > 1 ? ((canvasSizeX - spotWidth) % (spotsX - 1)) / 2 : 0;
            var yResolution = spotsY > 1 ? (canvasSizeY - spotHeight) / (spotsY - 1) : 0;
            var yRemainingOffset = spotsY > 1 ? ((canvasSizeY - spotHeight) % (spotsY - 1)) / 2 : 0;

            var counter = 0;
            var relationIndex = spotsX - spotsY + 1;

            for (var j = 0; j < spotsY; j++)
            {
                for (var i = 0; i < spotsX; i++)
                {
                    var isFirstColumn = i == 0;
                    var isLastColumn = i == spotsX - 1;
                    var isFirstRow = j == 0;
                    var isLastRow = j == spotsY - 1;

                    if (isFirstColumn || isLastColumn || isFirstRow || isLastRow) // needing only outer spots
                    {
                        var x = (xRemainingOffset + borderDistanceX + userSettings.OffsetX / scalingFactor + i * xResolution)
                                .Clamp(0, screenWidth);

                        var y = (yRemainingOffset + borderDistanceY + userSettings.OffsetY / scalingFactor + j * yResolution)
                                .Clamp(0, screenHeight);

                        var index = counter++; // in first row index is always counter

                        if (spotsX > 1 && spotsY > 1)
                        {
                            if (!isFirstRow && !isLastRow)
                            {
                                if (isFirstColumn)
                                {
                                    index += relationIndex + ((spotsY - 1 - j) * 3);
                                }
                                else if (isLastColumn)
                                {
                                    index -= j;
                                }
                            }

                            if (!isFirstRow && isLastRow)
                            {
                                index += relationIndex - i * 2;
                            }
                        }

                        devicespots[index] = new DeviceSpot(x, y, spotWidth, spotHeight,0,0,0,0,index);
                    }
                }
            }
            if (DeviceSettings.SelectedEffect==0)
            {


                if (DeviceSettings.SelectedDisplay == 0)
                {
                    if (GeneralSettings.OffsetLed != 0) Offset(ref devicespots, GeneralSettings.OffsetLed);
                }
                else if (DeviceSettings.SelectedDisplay == 1)
                {
                    if (GeneralSettings.OffsetLed2 != 0) Offset(ref devicespots, GeneralSettings.OffsetLed2);
                }
                else if (DeviceSettings.SelectedDisplay == 2)
                {
                    if (GeneralSettings.OffsetLed3 != 0) Offset(ref devicespots, GeneralSettings.OffsetLed3);
                }
            }
            if (userSettings.OffsetLed != 0) Offset(ref devicespots, userSettings.OffsetLed);
            if (spotsY > 1 && GeneralSettings.MirrorX) MirrorX(devicespots, spotsX, spotsY);
            if (spotsX > 1 && GeneralSettings.MirrorY) MirrorY(devicespots, spotsX, spotsY);

            devicespots[0].IsFirst = true;

            return devicespots;
        }

        internal IDeviceSpot[] BuildDeviceSpots(IDeviceSettings deviceSettings, IGeneralSettings generalSettings) // general settings is for compare each device setting
        {
            //import all User Defined Setting
            var rectWidth = deviceSettings.DeviceRectWidth;
            var rectHeight = deviceSettings.DeviceRectHeight;
            var DeviceLayout = deviceSettings.DeviceLayout;
            var DisplayRectWidth = 200;
            var DisplayRectHeight = 200;
            if(rectWidth>=rectHeight)
            {
                DisplayRectWidth = 360;
                DisplayRectHeight = 360 * rectHeight / rectWidth;
            }
            else
            {
                DisplayRectHeight = 360;
                DisplayRectWidth = 360 * rectWidth / rectHeight;
            }
            var numLED = deviceSettings.NumLED;
           IDeviceSpot[] deviceSpots = new DeviceSpot[numLED];

            switch(DeviceLayout)
            {
                case "Square":

                    deviceSpots=BuildSquare(numLED,rectWidth,rectHeight, DisplayRectWidth, DisplayRectHeight, deviceSettings);

                    break;
                case "Strip":

                    deviceSpots = BuildStrip(numLED, rectWidth, rectHeight, DisplayRectWidth, DisplayRectHeight, deviceSettings);

                    break;
                case "Rectangle":

                    BuildRectangle(numLED);

                    break;
                case "Matrix":

                    BuildMatrix(numLED);

                    break;
               



            }

            return deviceSpots;
        }

       
        private IDeviceSpot[] BuildSquare(int numSpot, int rectwidth, int rectheight, int displayRectWidth, int displayRectHeight, IDeviceSettings deviceSettings)
        {
            if(numSpot%4 !=0)
            {
                numSpot = numSpot + (4 - numSpot / 4); //square type always have multiple of 4 spot number
            }
            IDeviceSpot[] spotSet= new DeviceSpot[numSpot];
            var spotsX = (numSpot / 4) +1; // number of spot on one side
            var spotsY = (numSpot / 4)+1; // number of spot on one side
            var spotwidth = rectwidth / spotsX;
            var spotheight = rectheight/spotsY ;
            var displaySpotWidth = displayRectWidth / spotsX;
            var displaySpotHeight = displayRectHeight/spotsY;
            var counter = 0;
            var relationIndex = spotsX - spotsY + 1;
            for (var j = 0; j < spotsY; j++)
            {
                for (var i = 0; i < spotsX; i++)
                {
                    var isFirstColumn = i == 0;
                    var isLastColumn = i == spotsX - 1;
                    var isFirstRow = j == 0;
                    var isLastRow = j == spotsY - 1;

                    if (isFirstColumn || isLastColumn || isFirstRow || isLastRow) // needing only outer spots
                    {
                        var x = i * spotwidth;
                        var x1 = i * displaySpotWidth;

                        var y = j * spotheight;
                        var y1 = j * displaySpotHeight;


                        var index = counter++; // in first row index is always counter

                        if (spotsX > 1 && spotsY > 1)
                        {
                            if (!isFirstRow && !isLastRow)
                            {
                                if (isFirstColumn)
                                {
                                    index += relationIndex + ((spotsY - 1 - j) * 3);
                                }
                                else if (isLastColumn)
                                {
                                    index -= j;
                                }
                            }

                            if (!isFirstRow && isLastRow)
                            {
                                index += relationIndex - i * 2;
                            }
                        }

                        spotSet[index] = new DeviceSpot(x, y, spotwidth, spotheight,x1,y1,displaySpotWidth, displaySpotHeight, index);
                    }
                }
            }
            if (deviceSettings.OffsetLed != 0) Offset(ref spotSet, deviceSettings.OffsetLed);
            if (spotsY > 1 && deviceSettings.MirrorX) MirrorX(spotSet, spotsX, spotsY);
            if (spotsX > 1 && deviceSettings.MirrorY) MirrorY(spotSet, spotsX, spotsY);

            spotSet[0].IsFirst = true;
            int id= 0;
            foreach(var spot in spotSet)
            {
                spot.ID = (id++).ToString();
            }

            return spotSet;

           

        }
        private IDeviceSpot[] BuildRectangle(int numSpot)
        {
            var spotSet = new DeviceSpot[numSpot];
            return spotSet;

        }
        private IDeviceSpot[] BuildStrip(int numSpot, int rectwidth, int rectheight, int displayRectWidth, int displayRectHeight, IDeviceSettings deviceSettings)
        {
            if (numSpot >120)
            {
                numSpot = 120; //strip type only support 120 leds since the resolution of the shader is 120
            }
            IDeviceSpot[] spotSet = new DeviceSpot[numSpot];
            var spotsX = numSpot; // number of spot on one side
            var spotsY = 1; // number of spot on one side
            var spotwidth = rectwidth / spotsX;
            var spotheight = rectheight / spotsY;
            var displaySpotWidth = displayRectWidth / spotsX;
            var displaySpotHeight = displayRectHeight / spotsY;
            var counter = 0;
            var relationIndex = spotsX - spotsY + 1;
            
            for(var i=0;i<spotsX;i++)
            {
                var x = i * spotwidth;
                var x1 = i * displaySpotWidth;
                var y = 0;// strip layout only has 1 row
                var y1 = 0;
                var index = counter++;
                spotSet[index] = new DeviceSpot(x, y, spotwidth, spotheight, x1, y1, displaySpotWidth, displaySpotHeight, index);
            }
        
           // if (deviceSettings.OffsetLed != 0) Offset(ref spotSet, deviceSettings.OffsetLed); // offsetLED is obsolete
            if (spotsY > 1 && deviceSettings.MirrorX) MirrorX(spotSet, spotsX, spotsY);
           // if (spotsX > 1 && deviceSettings.MirrorY) MirrorY(spotSet, spotsX, spotsY); //mirror Y is obsolete

            spotSet[0].IsFirst = true;
            int id = 0;
            foreach (var spot in spotSet)
            {
                spot.ID = (id++).ToString();
            }

            return spotSet;


        }
        private IDeviceSpot[] BuildMatrix(int numSpot)
        {
            var spotSet = new DeviceSpot[numSpot];
            return spotSet;

        }


        private static void Mirror(IDeviceSpot[] spots, int startIndex, int length)
        {
            var halfLength = (length / 2);
            var endIndex = startIndex + length - 1;

            for (var i = 0; i < halfLength; i++)
            {
                spots.Swap(startIndex + i, endIndex - i);
            }
        }

        private static void MirrorX(IDeviceSpot[] spots, int spotsX, int spotsY)
        {
            // copy swap last row to first row inverse
            for (var i = 0; i < spotsX; i++)
            {
                var index1 = i;
                var index2 = (spots.Length - 1) - (spotsY - 2) - i;
                spots.Swap(index1, index2);
            }

            // mirror first column
            Mirror(spots, spotsX, spotsY - 2);

            // mirror last column
            if (spotsX > 1)
                Mirror(spots, 2 * spotsX + spotsY - 2, spotsY - 2);
        }

        private static void MirrorY(IDeviceSpot[] spots, int spotsX, int spotsY)
        {
            // copy swap last row to first row inverse
            for (var i = 0; i < spotsY - 2; i++)
            {
                var index1 = spotsX + i;
                var index2 = (spots.Length - 1) - i;
                spots.Swap(index1, index2);
            }

            // mirror first row
            Mirror(spots, 0, spotsX);

            // mirror last row
            if (spotsY > 1)
                Mirror(spots, spotsX + spotsY - 2, spotsX);
        }

        private static void Offset(ref IDeviceSpot[] spots, int offset)
        {
            IDeviceSpot[] temp = new DeviceSpot[spots.Length];
            for (var i = 0; i < spots.Length; i++)
            {
                temp[(i + temp.Length + offset) % temp.Length] = spots[i];
            }
            spots = temp;
        }

        public void IndicateMissingValues()
        {
            foreach (var spot in Spots)
            {
                spot.IndicateMissingValue();
            }
        }



    }


}
