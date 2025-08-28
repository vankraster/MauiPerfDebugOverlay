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
                builder.Services.AddSingleton<ProfilingHttpClient>();
                builder.Services.AddSingleton<HttpClient>(sp => sp.GetRequiredService<ProfilingHttpClient>()); 
            }

            return builder;
        }
    }
}
