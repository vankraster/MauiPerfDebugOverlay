namespace MauiPerfDebugOverlay.Interfaces
{
    public interface IFpsService
    {
        event Action<double> OnFrameTimeCalculated; // frame time în ms
        void Start();
        void Stop();
    }
}
