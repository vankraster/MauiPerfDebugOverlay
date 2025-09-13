using MauiPerfDebugOverlay.Models;
using MauiPerfDebugOverlay.Services;
using Microsoft.Maui.Handlers;
using System.Diagnostics;
using System.Diagnostics.Metrics;

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

                            //// -------------------------
                            //// 2️⃣ Track ScrollView metrics
                            //// -------------------------
                            //if (ve is ScrollView scrollView)
                            //{
                            //    double lastScrollY = 0;
                            //    DateTime lastTime = DateTime.UtcNow;

                            //    scrollView.Scrolled += (s, e) =>
                            //    {
                            //        var now = DateTime.UtcNow;
                            //        double deltaY = e.ScrollY - lastScrollY;
                            //        lastScrollY = e.ScrollY;

                            //        double deltaTime = (now - lastTime).TotalSeconds;
                            //        lastTime = now;

                            //        // dacă deltaTime e prea mare, ignorăm (user a stat inactiv)
                            //        if (deltaTime > 1.0) deltaTime = 0.001;

                            //        double velocity = deltaY / deltaTime;
                            //        bool jank = Math.Abs(velocity) < 1000;

                            //        ScrollMetricsBuffer.Instance.UpdateMetrics(
                            //            scrollView.Id,
                            //            "ScrollView",
                            //            deltaTime * 1000, // milisecunde
                            //            velocity,
                            //            jank
                            //        );
                            //    };
                            //}

                            //// -------------------------
                            //// 3️⃣ Track CollectionView metrics
                            //// -------------------------
                            //if (ve is CollectionView collectionView)
                            //{
                            //    double lastScrollOffset = 0;
                            //    DateTime lastTime = DateTime.UtcNow;

                            //    collectionView.Scrolled += (s, e) =>
                            //    {
                            //        var now = DateTime.UtcNow;
                            //        double deltaOffset = e.VerticalOffset - lastScrollOffset;
                            //        lastScrollOffset = e.VerticalOffset;

                            //        double deltaTime = (now - lastTime).TotalSeconds;
                            //        lastTime = now;

                            //        // ignorăm perioadele mari de inactivitate
                            //        if (deltaTime > 1.0) return;

                            //        double velocity = deltaOffset / (deltaTime + 0.001);
                            //        bool jank = Math.Abs(velocity) < 1000;

                            //        ScrollMetricsBuffer.Instance.UpdateMetrics(
                            //            collectionView.Id,
                            //            "CollectionView",
                            //            deltaTime * 1000, // milisecunde
                            //            velocity,
                            //            jank
                            //        );
                            //    };
                            //}
                        };
                    }
                });

            }




            var listener = new MeterListener();
            listener.InstrumentPublished += (instrument, l) =>
            {
                // ascultăm DOAR metricile de scrolling
                if (instrument.Meter.Name.Contains("Microsoft.Maui") &&
                    instrument.Name.StartsWith("maui.scrolling"))
                {
                    l.EnableMeasurementEvents(instrument);
                }
            };

            // pentru metrici de tip int (count, duration, jank)
            listener.SetMeasurementEventCallback<int>((instrument, measurement, tags, state) =>
            {
                Console.WriteLine($"[INT] {instrument.Name} = {measurement}");
                foreach (var tag in tags)
                {
                    Console.WriteLine($"   Tag: {tag.Key} = {tag.Value}");
                }
            });

            // pentru metrici de tip double (velocity)
            listener.SetMeasurementEventCallback<double>((instrument, measurement, tags, state) =>
            {
                Console.WriteLine($"[DOUBLE] {instrument.Name} = {measurement}");
                foreach (var tag in tags)
                {
                    Console.WriteLine($"   Tag: {tag.Key} = {tag.Value}");
                }
            });

            listener.Start();
            return builder;
        }
    }
}