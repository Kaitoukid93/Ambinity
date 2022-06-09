namespace adrilight
{
     public enum State { sleep, sentry, normal };
    public interface ISerialStream
    {
        bool IsRunning { get; }
        
        void Start();
        void Stop();
        bool IsValid();
        void DFU();
        State CurrentState { get; set; }


    }
}