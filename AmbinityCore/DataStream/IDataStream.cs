using AmbinityCore.Models.Device.Controller;

namespace AmbinityCore.DataStream;

public interface IDataStream
{
   
    event Action<IController> ControllerDisconnected;
    /// <summary>
    /// Indicate running state of this data stream
    /// </summary>
    bool IsRunning { get; }
    /// <summary>
    /// ID of this stream 
    /// </summary>
    string ID { get; set; }
    /// <summary>
    /// Initialize this stream with specific controller
    /// </summary>
    /// <param name="controller"></param>
    void Init();
    /// <summary>
    /// Start send data to the controller
    /// </summary>
    void Start();
    /// <summary>
    /// stop send data to the controller
    /// </summary>
    void Stop();
    /// <summary>
    /// Validation
    /// </summary>
    /// <returns></returns>
    bool IsValid();
}