using CoreAnimation;
using Foundation;
using MauiPerfDebugOverlay.Interfaces;

namespace MauiPerfDebugOverlay
{ 
    public class FpsService: IFpsService
    {
        private CADisplayLink _displayLink;
        private double _lastTimestamp;
        public event Action<double>? OnFrameTimeCalculated;

        public void Start()
        {
            _displayLink = CADisplayLink.Create(() =>
            {
                if (_lastTimestamp > 0)
                {
                    double frameTimeMs = (_displayLink.Timestamp - _lastTimestamp) * 1000.0;
                    OnFrameTimeCalculated?.Invoke(frameTimeMs);
                }
                _lastTimestamp = _displayLink.Timestamp;
            });
            _displayLink.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Default);
        }

        public void Stop()
        {
            _displayLink?.Invalidate();
            _displayLink = null;
        }
    }
}