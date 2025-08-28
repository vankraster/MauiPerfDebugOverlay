using MauiPerfDebugOverlay.Models;
using MauiPerfDebugOverlay.Services;

namespace MauiPerfDebugOverlay.Extensions
{
    public static class PerformanceDebugOverlayExtensions
    {
        internal static PerformanceOverlayOptions PerformanceOverlayOptions;

        public static MauiAppBuilder UsePerformanceDebugOverlay(this MauiAppBuilder builder, PerformanceOverlayOptions options)
        {
            PerformanceOverlayOptions = options;

            if (options.ShowNetworkStats)
            {
                HttpClientInterceptor.Initialize();
                //WebRequestInterceptor.Initialize();
            }

            return builder;
        }
    }
}
