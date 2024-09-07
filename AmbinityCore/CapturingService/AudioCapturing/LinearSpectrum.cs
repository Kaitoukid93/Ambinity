namespace AmbinityCore.CapturingService.AudioCapturing;

public class LinearSpectrum : AbstractSpectrum
{
    #region Constructors
    private float _speed1 = 1.0F, _speed2 = 0.20F;
    public LinearSpectrum(float[] data,float[] lastData, int bands, float minFrequency = -1, float maxFrequency = -1)
    {
        int dataReferenceCount = (data.Length - 1) * 2;

        int rawfromIndex = minFrequency < 0
            ? 0
            : FrequencyHelper.GetIndexOfFrequency(minFrequency, dataReferenceCount);
        int fromIndex =
            Math.Clamp(rawfromIndex, 0,
                data.Length - 1 - bands); // -bands since we need at least enough data to get our bands)  
        int rawtoIndex = maxFrequency < 0
            ? data.Length - 1
            : FrequencyHelper.GetIndexOfFrequency(maxFrequency, dataReferenceCount);
        int toIndex = Math.Clamp(rawtoIndex,fromIndex, data.Length - 1);

        int usableSourceData = Math.Max(bands, (toIndex - fromIndex) + 1);

        Bands = new Band[bands];

        double frequenciesPerBand = (double)usableSourceData / bands;
        double frequencyCounter = 0;

        int index = fromIndex;
        for (int i = 0; i < Bands.Length; i++)
        {
           
            frequencyCounter += frequenciesPerBand;
            int count = (int)frequencyCounter;
            //damp the value
            for (int j = 0; j < count; j++)
            {
                var offset = i * (int)frequenciesPerBand;
                if (data[offset+j] > lastData[offset+j])
                {
                    lastData[offset+j] += (byte)(_speed1 * (data[offset+j] - lastData[offset+j]));
                }

                if (data[offset+j] < lastData[offset+j])
                {
                    lastData[offset+j] -= (byte)(_speed2 * (lastData[offset+j] - data[offset+j]));
                }
            }
              
            float[] bandData = new float[count];
            Array.Copy(lastData, index, bandData, 0, count);
            
            Bands[i] = new Band(FrequencyHelper.GetFrequencyOfIndex(index, dataReferenceCount),
                FrequencyHelper.GetFrequencyOfIndex(index + count, dataReferenceCount),
                bandData);

            index += count;
            frequencyCounter -= count;
        }
    }

    #endregion
}