using Draw2D.Core.Graphic;

namespace AmbinityCore.CapturingService.AudioCapturing;

/// <summary>
/// contains all audio data currently available in the system, ffs I don't want to do this but this is the only way to make it work
/// </summary>
public class AudioBuffer
{
    #region Properties & Fields
    private byte[][] _buffer;
    private int _capacity;

    public int Size => _capacity;

    #endregion

    #region Constructors

    public AudioBuffer(int capacity)
    {
        this._capacity = capacity;
        _buffer = new byte[capacity][];
        for (int i = 0; i < capacity; i++)
        {
            _buffer[i] = new byte[32];
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