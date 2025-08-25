using System;
using Tizen.Applications;
using Tizen.NUI;

namespace MauiPerfDebugOverlay
{
    public class FpsService : IFpsService
    {
        private Animation _anim;
        private long _lastTicks;
        public event Action<double>? OnFrameTimeCalculated;

        public void Start()
        {
            _lastTicks = 0;
            _anim = new Animation(1);
            _anim.EndAction = Animation.EndActions.StopFinal;
            _anim.Finished += (s, e) =>
            {
                long now = DateTime.UtcNow.Ticks;
                if (_lastTicks > 0)
                {
                    double frameTimeMs = (now - _lastTicks) / TimeSpan.TicksPerMillisecond;
                    OnFrameTimeCalculated?.Invoke(frameTimeMs);
                }
                _lastTicks = now;
                _anim.Play(); // reloop
            };
            _anim.Play();
        }

        public void Stop()
        {
            _anim?.Stop();
            _anim?.Dispose();
            _anim = null;
        }
    }

}