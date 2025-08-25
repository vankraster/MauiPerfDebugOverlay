using MauiPerfDebugOverlay.Interfaces;
using Microsoft.UI.Xaml.Media;

namespace MauiPerfDebugOverlay
{ 
    public class FpsService : IFpsService
    {
        private TimeSpan _last;
        public event Action<double>? OnFrameTimeCalculated;

        public void Start()
        {
            CompositionTarget.Rendering += OnRendering;
        }

        public void Stop()
        {
            CompositionTarget.Rendering -= OnRendering;
        }

        private void OnRendering(object sender, object e)
        {
            if (e is RenderingEventArgs args)
            {
                if (_last != TimeSpan.Zero)
                {
                    double frameTimeMs = (args.RenderingTime - _last).TotalMilliseconds;
                    OnFrameTimeCalculated?.Invoke(frameTimeMs);
                }
                _last = args.RenderingTime;
            }
        }
    }

}
