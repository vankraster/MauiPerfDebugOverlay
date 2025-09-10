using System;
using System.Collections.Generic;

namespace MauiPerfDebugOverlay.Services
{
    /// <summary>
    /// Singleton service care stochează metricile de scroll pentru controale MAUI.
    /// </summary>
    public class ScrollMetricsBuffer
    {
        private static ScrollMetricsBuffer _instance;
        public static ScrollMetricsBuffer Instance => _instance ??= new ScrollMetricsBuffer();

        // Cheia este Id-ul controlului
        private readonly Dictionary<string, ScrollMetrics> _metrics = new();

        private ScrollMetricsBuffer() { }

        /// <summary>
        /// Actualizează metricile unui control
        /// </summary>
        /// <param name="controlId">Id-ul controlului</param>
        /// <param name="durationMs">Durata scroll-ului în milisecunde</param>
        /// <param name="velocity">Viteza scroll-ului în pixeli/secundă</param>
        /// <param name="jank">Dacă frame-ul a depășit 16ms</param>
        public void UpdateMetrics(string controlId, double durationMs, double velocity, bool jank)
        {
            lock (_metrics)
            {
                if (!_metrics.TryGetValue(controlId, out var metric))
                {
                    metric = new ScrollMetrics();
                    _metrics[controlId] = metric;
                }

                metric.Count++;
                metric.TotalDurationMs += durationMs;
                metric.Velocity = velocity; // ultimele valori
                if (jank) metric.JankCount++;
            }
        }

        /// <summary>
        /// Obține metricile pentru un control
        /// </summary>
        /// <param name="controlId">Id-ul controlului</param>
        /// <returns>ScrollMetrics sau null dacă nu există</returns>
        public ScrollMetrics? GetMetrics(string controlId)
        {
            lock (_metrics)
            {
                return _metrics.TryGetValue(controlId, out var metric) ? metric : null;
            }
        }

        /// <summary>
        /// Resetează metricile pentru un control specific
        /// </summary>
        public void ClearMetrics(string controlId)
        {
            lock (_metrics)
            {
                _metrics.Remove(controlId);
            }
        }

        /// <summary>
        /// Resetează metricile pentru toate controalele
        /// </summary>
        public void ClearAllMetrics()
        {
            lock (_metrics)
            {
                _metrics.Clear();
            }
        }
    }

    /// <summary>
    /// Metricile colectate pentru un scroll
    /// </summary>
    public class ScrollMetrics
    {
        /// <summary>Numărul de evenimente scroll înregistrate</summary>
        public int Count { get; set; } = 0;

        /// <summary>Durata totală în milisecunde</summary>
        public double TotalDurationMs { get; set; } = 0;

        /// <summary>Viteza ultimei derulări în pixeli/sec</summary>
        public double Velocity { get; set; } = 0;

        /// <summary>Numărul de derulări jank (>16ms)</summary>
        public int JankCount { get; set; } = 0;

        /// <summary>Durata medie a scroll-urilor</summary>
        public double AvgDuration => Count > 0 ? TotalDurationMs / Count : 0;
    }
}
