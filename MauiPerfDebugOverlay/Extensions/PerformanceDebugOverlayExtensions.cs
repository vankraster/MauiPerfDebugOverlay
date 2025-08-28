using MauiPerfDebugOverlay.Models;
using MauiPerfDebugOverlay.Services;
using static MauiPerfDebugOverlay.Services.NetworkProfiler;

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
                //builder.Services.AddSingleton(sp =>
                //{
                //    var handler = NetworkProfiler.Instance.CreateHandler(new HttpClientHandler());
                //    return new HttpClient(handler);
                //});

                builder.Services.AddSingleton<ProfilingHttpClient>();
                builder.Services.AddSingleton<HttpClient>(sp => sp.GetRequiredService<ProfilingHttpClient>());


                //HttpClientInterceptor.Initialize();
            }

            return builder;
        }
    }
}
