using Android.Views;
using MauiPerfDebugOverlay.Interfaces;

namespace MauiPerfDebugOverlay.Platforms
{ 
    public class FpsService : Java.Lang.Object, IFpsService, Choreographer.IFrameCallback
    {
        private long _lastFrameTimeNanos = 0;
        public event Action<double>? OnFrameTimeCalculated;

        public void Start()
        {
            _lastFrameTimeNanos = 0;
            Choreographer.Instance.PostFrameCallback(this);
        }

        public void Stop()
        {
            Choreographer.Instance.RemoveFrameCallback(this);
        }

        public void DoFrame(long frameTimeNanos)
        {
            if (_lastFrameTimeNanos > 0)
            {
                double frameTimeMs = (frameTimeNanos - _lastFrameTimeNanos) / 1_000_000.0;
                OnFrameTimeCalculated?.Invoke(frameTimeMs);
            }
            _lastFrameTimeNanos = frameTimeNanos;
            Choreographer.Instance.PostFrameCallback(this);
        }
    }
}