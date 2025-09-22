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
        private readonly Dictionary<Guid, ScrollMetrics> _metrics = new();
        private readonly Lock _lock = new();
        /// <summary>
        /// Raised whenever the metrics collection changes.
        /// Arguments: action type ("Add" or "Clear"), element Id, load time (if applicable).
        /// </summary>
        public event Action<string, Guid?, double?, double?, bool?>? CollectionChanged;


        private ScrollMetricsBuffer() { }

        /// <summary>
        /// Actualizează metricile unui control
        /// </summary>
        /// <param name="controlId">Id-ul controlului</param>
        /// <param name="durationMs">Durata scroll-ului în milisecunde</param>
        /// <param name="velocity">Viteza scroll-ului în pixeli/secundă</param>
        /// <param name="jank">Dacă frame-ul a depășit 16ms</param>
        public void UpdateMetrics(Guid controlId, string type, double durationMs, double velocity, bool jank)
        {
            using (_lock.EnterScope())
            {
                if (!_metrics.TryGetValue(controlId, out var metric))
                {
                    metric = new ScrollMetrics();
                    _metrics[controlId] = metric;
                }

                metric.Count++;
                metric.TotalDurationMs += durationMs;
                metric.Velocity = velocity; // ultimele valori
                metric.Type = type;
                if (jank) metric.JankCount++;

                CollectionChanged?.Invoke("Add", controlId, durationMs, velocity, jank);
            }
        }

        /// <summary>
        /// Obține metricile pentru un control
        /// </summary>
        /// <param name="controlId">Id-ul controlului</param>
        /// <returns>ScrollMetrics sau null dacă nu există</returns>
        public ScrollMetrics? GetMetrics(Guid controlId)
        {
            using (_lock.EnterScope())
            {
                return _metrics.TryGetValue(controlId, out var metric) ? metric : null;
            }
        }

        /// <summary>
        /// Resetează metricile pentru un control specific
        /// </summary>
        public void ClearMetrics(Guid controlId)
        {
            using (_lock.EnterScope())
            {
                _metrics.Remove(controlId);
            }

            CollectionChanged?.Invoke("Clear", null, null, null, null);
        }

        /// <summary>
        /// Resetează metricile pentru toate controalele
        /// </summary>
        public void ClearAllMetrics()
        {
            using (_lock.EnterScope())
            {
                _metrics.Clear();
            }

            CollectionChanged?.Invoke("Clear", null, null, null, null);
        }

        public List<Guid> GetAllControlIds()
        {
            using (_lock.EnterScope())
            {
                return _metrics.Keys.ToList();
            }
        }
    }

    /// <summary>
    /// Metricile colectate pentru un scroll
    /// </summary>
    public class ScrollMetrics
    {
        public string Type { get; set; } = "";

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
