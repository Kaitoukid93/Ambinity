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

                    //if ((DeviceSettings.SpotsX + DeviceSettings.SpotsY) * 2 - 4 != DeviceSettings.NumLED)//missmatch
                    //{
                    //    DeviceSettings.SpotsY = ((DeviceSettings.NumLED + 4) / 2) - DeviceSettings.SpotsX;
                    //    if(DeviceSettings.SpotsY<=1||DeviceSettings.SpotsX<=1)
                    //    {
                    //        HandyControl.Controls.MessageBox.Show("Số LED mỗi cạnh không thể nhỏ hơn 1");
                    //        DeviceSettings.SpotsY = 1;
                    //        DeviceSettings.SpotsX = DeviceSettings.NumLED;
                    //    }


                    //}
                    //Refresh();
                    //break;
                case nameof(DeviceSettings.SpotsY):
                    
                    //if ((DeviceSettings.SpotsX + DeviceSettings.SpotsY) * 2 - 4 != DeviceSettings.NumLED)//missmatch
                    //{
                    //    DeviceSettings.SpotsX = ((DeviceSettings.NumLED + 4) / 2) - DeviceSettings.SpotsY;
                    //    if (DeviceSettings.SpotsY <= 1 || DeviceSettings.SpotsX <= 1)
                    //    {
                    //        HandyControl.Controls.MessageBox.Show("Số LED mỗi cạnh không thể nhỏ hơn 1");
                    //        DeviceSettings.SpotsY = 1;
                    //        DeviceSettings.SpotsX = DeviceSettings.NumLED;
                    //    }

                    //}
                    //Refresh();
                    //break;
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
                case nameof(DeviceSettings.DeviceRectWidth1):
                case nameof(DeviceSettings.DeviceRectHeight1):
                case nameof(DeviceSettings.NumLED):
                case nameof(DeviceSettings.DeviceLayout):
                case nameof(DeviceSettings.DeviceRotation):
                case nameof(DeviceSettings.DeviceRectLeft):
                case nameof(DeviceSettings.DeviceRectTop):
                case nameof(DeviceSettings.DeviceRectLeft1):
                case nameof(DeviceSettings.DeviceRectTop1):
                case nameof(DeviceSettings.MatrixOrientation):
                case nameof(DeviceSettings.MatrixStartPoint):


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
       

        public int ID {
            get { return DeviceSettings.DeviceID; }
           
        }
     

      

        public int ParrentLocation {
            get { return DeviceSettings.ParrentLocation; }
           
        }

              public string DeviceSerial {
            get { return DeviceSettings.DeviceSerial; }
           
        }
        public string DeviceLocation {
            get { return DeviceSettings.DevicePort; }

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
          
        }

        internal IDeviceSpot[] BuildDeviceSpots(IDeviceSettings deviceSettings, IGeneralSettings generalSettings) // general settings is for compare each device setting
        {
            //import all User Defined Setting
            int rectWidth;
            int rectHeight;
            switch(deviceSettings.SelectedEffect)
            {
                case 0:
                    rectWidth = deviceSettings.DeviceRectWidth1;
                    rectHeight = deviceSettings.DeviceRectHeight1;
                break;
                case 5:
                    rectWidth = deviceSettings.DeviceRectWidth;
                    rectHeight = deviceSettings.DeviceRectHeight;
                    break;
                default:
                    rectWidth = deviceSettings.DeviceRectWidth;
                    rectHeight = deviceSettings.DeviceRectHeight;
                    break;

            }
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
                case 0:

                    deviceSpots = BuildRectangle(numLED, rectWidth, rectHeight, DisplayRectWidth, DisplayRectHeight, deviceSettings);

                    break;
                case 1:

                    deviceSpots = BuildStrip(numLED, rectWidth, rectHeight, DisplayRectWidth, DisplayRectHeight, deviceSettings);

                    break;
                case 2:

                    deviceSpots = BuildMatrix(numLED, rectWidth, rectHeight, DisplayRectWidth, DisplayRectHeight, deviceSettings);

                    break;
               



            }

            return deviceSpots;
        }

       
        
        private IDeviceSpot[] BuildRectangle(int numSpot, int rectwidth, int rectheight, int displayRectWidth, int displayRectHeight, IDeviceSettings deviceSettings)
        {
           
            
            var spotsX = deviceSettings.SpotsX; // number of spot on one side
            var spotsY = deviceSettings.SpotsY; // number of spot on one side   
            IDeviceSpot[] spotSet = new DeviceSpot[CountLeds(spotsX,spotsY)];
            var spotwidth = rectwidth / spotsX;
            var spotheight = rectheight / spotsY;
            var displaySpotWidth = displayRectWidth / spotsX;
            var displaySpotHeight = displayRectHeight / spotsY;
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

                        spotSet[index] = new DeviceSpot(x, y, spotwidth, spotheight, x1, y1, displaySpotWidth, displaySpotHeight, index);
                    }
                }
            }
            if (deviceSettings.OffsetLed != 0) Offset(ref spotSet, deviceSettings.OffsetLed);
            if (spotsY > 1 && deviceSettings.MirrorX) MirrorX(spotSet, spotsX, spotsY);
            if (spotsX > 1 && deviceSettings.MirrorY) MirrorY(spotSet, spotsX, spotsY);

            spotSet[0].IsFirst = true;
            int id = 0;
            foreach (var spot in spotSet)
            {
                spot.ID = (id++).ToString();
            }

            return spotSet;


        }
        private IDeviceSpot[] BuildStrip(int numSpot, int rectwidth, int rectheight, int displayRectWidth, int displayRectHeight, IDeviceSettings deviceSettings)
        {
            if (numSpot >120)
            {
                numSpot = 120; //strip type only support 120 leds since the resolution of the shader is 120
            }
            IDeviceSpot[] spotSet = new DeviceSpot[numSpot];
            int spotsX; 
            int spotsY; 
            if(deviceSettings.DeviceRotation ==0)
            {
                spotsX = numSpot;
                spotsY = 1;
            }
            else if (deviceSettings.DeviceRotation==1)
            {
                spotsY = numSpot;
                spotsX = 1;
            }
            else if (deviceSettings.DeviceRotation==2)
            {
                spotsX = numSpot;
                spotsY = 1;
            }
            else
            {
                spotsY = numSpot;
                spotsX = 1;

            }
            var spotwidth = rectwidth / spotsX;
            var spotheight = rectheight / spotsY;
            var displaySpotWidth = displayRectWidth / spotsX;
            var displaySpotHeight = displayRectHeight / spotsY;
            var counter = 0;
           // var relationIndex = spotsX - spotsY + 1;
            if (deviceSettings.DeviceRotation == 0 || deviceSettings.DeviceRotation == 2)
            {
                for (var i = 0; i < spotsX; i++)
                {
                    var x = i * spotwidth;
                    var x1 = i * displaySpotWidth;
                    var y = 0;// strip layout only has 1 row
                    var y1 = 0;
                    var index = counter++;
                    spotSet[index] = new DeviceSpot(x, y, spotwidth, spotheight, x1, y1, displaySpotWidth, displaySpotHeight, index);
                }
            }
            else
            {
                for (var i = 0; i < spotsY; i++)
                {
                    var x = 0;
                    var x1 = 0;
                    var y = i * spotheight;// strip layout only has 1 row
                    var y1 = i * displaySpotHeight;
                    var index = counter++;
                    spotSet[index] = new DeviceSpot(x, y, spotwidth, spotheight, x1, y1, displaySpotWidth, displaySpotHeight, index);
                }
            }
         

            // if (deviceSettings.OffsetLed != 0) Offset(ref spotSet, deviceSettings.OffsetLed); // offsetLED is obsolete
            if (deviceSettings.DeviceRotation == 2) Mirror(spotSet, 0, spotsX);
            if (deviceSettings.DeviceRotation == 3) Mirror(spotSet, 0, spotsY);

            spotSet[0].IsFirst = true;
            int id = 0;
            foreach (var spot in spotSet)
            {
                spot.ID = (id++).ToString();
            }

            return spotSet;


        }
        private IDeviceSpot[] BuildMatrix(int numSpot, int rectwidth, int rectheight, int displayRectWidth, int displayRectHeight, IDeviceSettings deviceSettings)
        {
            //if (numSpot > 120)
            //{
            //    numSpot = 120; //strip type only support 120 leds since the resolution of the shader is 120
            //}
            
            var spotsX = deviceSettings.SpotsX; // number of spot on one side
            var spotsY = deviceSettings.SpotsY; // number of spot on one side
            IDeviceSpot[] spotSet = new DeviceSpot[spotsX*spotsY];
            var spotwidth = rectwidth / spotsX;
            var spotheight = rectheight / spotsY;
            var displaySpotWidth = displayRectWidth / spotsX;
            var displaySpotHeight = displayRectHeight / spotsY;
            var counter = 0;
            var relationIndex = spotsX - spotsY + 1;
            switch (deviceSettings.MatrixStartPoint)
            {
                case 0: //start matrix at top left corner
                 switch(deviceSettings.MatrixOrientation)
                    {
                        case 0:
                            for (var j = 0; j < spotsY; j++)
                            {
                                for (var i = 0; i < spotsX; i++)
                                {



                                    var x = i * spotwidth;
                                    var x1 = i * displaySpotWidth;

                                    var y = j * spotheight;
                                    var y1 = j * displaySpotHeight;


                                    var index = counter;

                                    spotSet[index] = new DeviceSpot(x, y, spotwidth, spotheight, x1, y1, displaySpotWidth, displaySpotHeight, index);
                                    counter++;

                                }
                            }

                            break;
                        case 1:
                            for (var j = 0; j < spotsX; j++)
                            {
                                for (var i = 0; i < spotsY; i++)
                                {



                                    var x = j * spotwidth;
                                    var x1 = j * displaySpotWidth;

                                    var y = i * spotheight;
                                    var y1 = i * displaySpotHeight;


                                    var index = counter;

                                    spotSet[index] = new DeviceSpot(x, y, spotwidth, spotheight, x1, y1, displaySpotWidth, displaySpotHeight, index);
                                    counter++;

                                }
                            }

                            break;

                    }

                    break;
                case 1: // start matrix at the top right corner

                    switch (deviceSettings.MatrixOrientation)
                    {
                        case 0:
                            for (var j = 0; j < spotsY; j++)
                            {
                                for (var i = spotsX-1; i >=0; i--)
                                {



                                    var x = i * spotwidth;
                                    var x1 = i * displaySpotWidth;

                                    var y = j * spotheight;
                                    var y1 = j * displaySpotHeight;


                                    var index = counter;

                                    spotSet[index] = new DeviceSpot(x, y, spotwidth, spotheight, x1, y1, displaySpotWidth, displaySpotHeight, index);
                                    counter++;

                                }
                            }

                            break; 
                        case 1:
                            for (var i = spotsX-1; i >= 0; i--)
                            {
                                for (var j = 0; j < spotsY; j++)
                                {



                                    var x = i * spotwidth;
                                    var x1 = i * displaySpotWidth;

                                    var y = j * spotheight;
                                    var y1 = j * displaySpotHeight;


                                    var index = counter;

                                    spotSet[index] = new DeviceSpot(x, y, spotwidth, spotheight, x1, y1, displaySpotWidth, displaySpotHeight, index);
                                    counter++;

                                }
                            }

                            break;

                    }

                    break;

                case 2: // start matrix at the bottom right corner

                    switch (deviceSettings.MatrixOrientation)
                    {
                        case 0:
                            for (var j = spotsY-1; j >= 0; j--)
                            {
                                for (var i = spotsX-1; i >= 0; i--)
                                {



                                    var x = i * spotwidth;
                                    var x1 = i * displaySpotWidth;

                                    var y = j * spotheight;
                                    var y1 = j * displaySpotHeight;


                                    var index = counter;

                                    spotSet[index] = new DeviceSpot(x, y, spotwidth, spotheight, x1, y1, displaySpotWidth, displaySpotHeight, index);
                                    counter++;

                                }
                            }

                            break;
                        case 1:
                            for (var i = spotsX-1; i >= 0; i--)
                            {
                                for (var j = spotsY-1; j >= 0; j--)
                                {



                                    var x = i * spotwidth;
                                    var x1 = i * displaySpotWidth;

                                    var y = j * spotheight;
                                    var y1 = j * displaySpotHeight;


                                    var index = counter;

                                    spotSet[index] = new DeviceSpot(x, y, spotwidth, spotheight, x1, y1, displaySpotWidth, displaySpotHeight, index);
                                    counter++;

                                }
                            }

                            break;

                    }

                    break;

                case 3: // start matrix at the bottom left corner

                    switch (deviceSettings.MatrixOrientation)
                    {
                        case 0:
                            for (var j = spotsY - 1; j >= 0; j--)
                            {
                                for (var i = 0; i < spotsX; i++)
                                {



                                    var x = i * spotwidth;
                                    var x1 = i * displaySpotWidth;

                                    var y = j * spotheight;
                                    var y1 = j * displaySpotHeight;


                                    var index = counter;

                                    spotSet[index] = new DeviceSpot(x, y, spotwidth, spotheight, x1, y1, displaySpotWidth, displaySpotHeight, index);
                                    counter++;

                                }
                            }

                            break;
                        case 1:
                            for (var i = 0; i < spotsX; i++)
                            {
                                for (var j = spotsY - 1; j >= 0; j--)
                                {



                                    var x = i * spotwidth;
                                    var x1 = i * displaySpotWidth;

                                    var y = j * spotheight;
                                    var y1 = j * displaySpotHeight;


                                    var index = counter;

                                    spotSet[index] = new DeviceSpot(x, y, spotwidth, spotheight, x1, y1, displaySpotWidth, displaySpotHeight, index);
                                    counter++;

                                }
                            }

                            break;

                    }

                    break;


            }
           

            // if (deviceSettings.OffsetLed != 0) Offset(ref spotSet, deviceSettings.OffsetLed); // offsetLED is obsolete
            //if (spotsY > 1 && deviceSettings.MirrorX) MirrorX(spotSet, spotsX, spotsY);
            // if (spotsX > 1 && deviceSettings.MirrorY) MirrorY(spotSet, spotsX, spotsY); //mirror Y is obsolete

            spotSet[0].IsFirst = true;
            int id = 0;
            foreach (var spot in spotSet)
            {
                spot.ID = (id++).ToString();
            }

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
