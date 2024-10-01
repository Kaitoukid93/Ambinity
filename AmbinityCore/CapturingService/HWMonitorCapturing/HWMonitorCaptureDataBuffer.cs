namespace AmbinityCore.CapturingService.HWMonitorCapturing;

public class HWMonitorCaptureDataBuffer
{
    #region Properties & Fields
    private double[][] _buffer;
    private int _capacity;

    public int Size => _capacity;

    #endregion

    #region Constructors

    public HWMonitorCaptureDataBuffer(int capacity)
    {
        this._capacity = capacity;
        _buffer = new double[capacity][];
        for (int i = 0; i < capacity; i++)
        {
            _buffer[i] = new double[3]; //value, min , max
        }
    }

    #endregion

    #region Methods

    public void Put(int index, double[] data)
    {
        data.CopyTo(_buffer[index],0);
    }

    public double GetValue(int position, int index)
    {
        return _buffer[position][index];
    }
    public void CopyInto(int index,double[] reusableArray)
    {
        _buffer[index].CopyTo(reusableArray,0);
    }

    #endregion
}