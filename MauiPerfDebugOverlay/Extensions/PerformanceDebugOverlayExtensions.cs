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
                LoadTimeMetricsStore loadTimeMetricsStore = LoadTimeMetricsStore.Instance;
                //PageHandler.Mapper.AppendToMapping("ClearLoadTimeMetrics", (handler, view) =>
                //{
                //    // Clear metrics as soon as the page is created
                //    loadTimeMetricsStore.Clear();
                //});

                // Add metrics for load VisualElement (including layouts, controls)
                ViewHandler.ViewMapper.AppendToMapping("MeasureComponentLoad", (handler, view) =>
                {
                    if (view is VisualElement ve)
                    {
                        // -------------------------
                        // 1️⃣ Track load time
                        // -------------------------
                        var swHandlerChanged = Stopwatch.StartNew();

                        ve.HandlerChanged += (_, __) =>
                        {
                            if (swHandlerChanged.IsRunning)
                            {
                                swHandlerChanged.Stop();
                                loadTimeMetricsStore.Add(ve.Id, swHandlerChanged.Elapsed.TotalMilliseconds);
                            }

                            // -------------------------
                            // 2️⃣ Track ScrollView metrics
                            // -------------------------
                            if (ve is ScrollView scrollView)
                            {
                                double lastScrollY = 0;
                                Stopwatch swScroll = new Stopwatch();

                                scrollView.Scrolled += (s, e) =>
                                {
                                    if (!swScroll.IsRunning) swScroll.Start();

                                    double deltaY = e.ScrollY - lastScrollY;
                                    lastScrollY = e.ScrollY;

                                    double velocity = deltaY / (swScroll.Elapsed.TotalSeconds + 0.001);
                                    bool jank = swScroll.Elapsed.TotalMilliseconds > 16;

                                    ScrollMetricsBuffer.Instance.UpdateMetrics(
                                        scrollView.Id.ToString(),
                                        swScroll.Elapsed.TotalMilliseconds,
                                        velocity,
                                        jank
                                    );

                                    swScroll.Restart();
                                };
                            }

                            // -------------------------
                            // 3️⃣ Track CollectionView metrics
                            // -------------------------
                            if (ve is CollectionView collectionView)
                            {
                                double lastScrollOffset = 0;
                                Stopwatch swScroll = new Stopwatch();

                                collectionView.Scrolled += (s, e) =>
                                {
                                    if (!swScroll.IsRunning) swScroll.Start();

                                    double deltaOffset = e.VerticalOffset - lastScrollOffset;
                                    lastScrollOffset = e.VerticalOffset;

                                    double velocity = deltaOffset / (swScroll.Elapsed.TotalSeconds + 0.001);
                                    bool jank = swScroll.Elapsed.TotalMilliseconds > 16;

                                    ScrollMetricsBuffer.Instance.UpdateMetrics(
                                        collectionView.Id.ToString(),
                                        swScroll.Elapsed.TotalMilliseconds,
                                        velocity,
                                        jank
                                    );

                                    swScroll.Restart();
                                };
                            }
                        };
                    }
                });

            }


            return builder;
        }
    }
}