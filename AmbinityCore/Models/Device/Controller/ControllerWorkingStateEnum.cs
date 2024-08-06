namespace AmbinityCore.Models.Device.Controller;

public enum ControllerWorkingStateEnum
{
    /// <summary>
    /// Normal working state
    /// </summary>
    Normal,

    /// <summary>
    /// Device is in sleep mode
    /// </summary>
    Sleep,

    /// <summary>
    /// Device is off and not receiving data from the app
    /// </summary>
    Off,

    /// <summary>
    /// Device is in firmware upgrade mode
    /// </summary>
    DFU
}