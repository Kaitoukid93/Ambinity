namespace AmbinityCore.CapturingService.HWMonitorCapturing;

public class HWMonitorCaptureDataBuffer
{
    #region Properties & Fields
    private byte[][] _buffer;
    private int _capacity;

    public int Size => _capacity;

    #endregion

    #region Constructors

    public HWMonitorCaptureDataBuffer(int capacity)
    {
        this._capacity = capacity;
        _buffer = new byte[capacity][];
        for (int i = 0; i < capacity; i++)
        {
            _buffer[i] = new byte[3]; //value, min , max
        }
    }

    #endregion

    #region Methods

    public void Put(int index, byte[] data)
    {
        data.CopyTo(_buffer[index],0);
    }

    public byte GetByte(int position, int index)
    {
        return _buffer[position][index];
    }
    public void CopyInto(int index,byte[] reusableArray)
    {
        _buffer[index].CopyTo(reusableArray,0);
    }

    #endregion
}