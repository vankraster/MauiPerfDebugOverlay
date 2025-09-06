namespace MauiPerfDebugOverlay.Services
{
    /// <summary>
    /// Thread-safe storage for load times per element.
    /// Key = element Id, Value = load time in ms.
    /// </summary>
    internal class LoadTimeMetricsStore
    {
        private readonly Dictionary<Guid, double> _metrics = new();
        private readonly object _lock = new();

        /// <summary>
        /// Raised whenever the metrics collection changes.
        /// Arguments: action type ("Add" or "Clear"), element Id, load time (if applicable).
        /// </summary>
        public event Action<string, Guid?, double?>? CollectionChanged;

        public void Add(Guid id, double ms)
        {
            lock (_lock)
            {
                _metrics[id] = ms;
            }

            // Notify subscribers
            CollectionChanged?.Invoke("Add", id, ms);
        }

        public void Clear()
        {
            lock (_lock)
            {
                _metrics.Clear();
            }

            // Notify subscribers
            CollectionChanged?.Invoke("Clear", null, null);
        }

        public IReadOnlyDictionary<Guid, double> GetAll()
        {
            lock (_lock)
            {
                return new Dictionary<Guid, double>(_metrics);
            }
        }
    }
}