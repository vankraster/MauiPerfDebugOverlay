using MauiPerfDebugOverlay.Models;
using MauiPerfDebugOverlay.Services;
using Microsoft.Maui.Handlers;
using System.Diagnostics;

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


            if (options.ShowLoadTime)
            {
                LoadTimeMetricsStore loadTimeMetricsStore = new LoadTimeMetricsStore();
                PageHandler.Mapper.AppendToMapping("ClearLoadTimeMetrics", (handler, view) =>
                {
                    // Clear metrics as soon as the page is created
                    loadTimeMetricsStore.Clear();
                });

                // Add metrics for load VisualElement (including layouts, controls)
                ViewHandler.ViewMapper.AppendToMapping("MeasureComponentLoad", (handler, view) =>
                {
                    if (view is VisualElement ve)
                    {
                        #region track loading
                        var swLoaded = Stopwatch.StartNew();
                        //var swHandlerChanged = Stopwatch.StartNew();


                        //Here the difference is subtle but important.
                        //we want to know when the element is actually loaded and ready, not just when its handler is set ?!
                        //HandlerChanged can be too early in some cases
                        //Loaded can be too late in some cases (like if the element is never shown)

                        //ve.HandlerChanged += (_, __) =>
                        //{
                        //    if (swHandlerChanged.IsRunning)
                        //    {
                        //        swHandlerChanged.Stop();
                        //        overlay?.AddMetricElementLoad(ve.Id, ve.GetType().Name, swHandlerChanged.Elapsed.TotalMilliseconds);
                        //    }
                        //};

                        //here we only track the element when it is actually loaded (which means it is part of the visual tree and has a size)
                        ve.Loaded += (_, __) =>
                        {
                            if (swLoaded.IsRunning)
                            {
                                swLoaded.Stop();
                                loadTimeMetricsStore.Add(ve.Id, swLoaded.Elapsed.TotalMilliseconds);
                            }
                        };
                        #endregion
                    }
                });
            }


            return builder;
        }
    }
}