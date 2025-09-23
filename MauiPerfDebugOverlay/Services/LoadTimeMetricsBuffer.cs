using MauiPerfDebugOverlay.Models.Internal;

namespace MauiPerfDebugOverlay.Services
{
    /// <summary>
    /// Thread-safe storage for load times per element.
    /// Key = element Id, Value = load time in ms.
    /// </summary>
    internal class LoadTimeMetricsStore
    {
        private static readonly Lazy<LoadTimeMetricsStore> _instance = new(() => new LoadTimeMetricsStore());
        public static LoadTimeMetricsStore Instance => _instance.Value;

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

        public double? GetValue(Guid id)
        {
            lock (_lock)
            {
                return _metrics.TryGetValue(id, out var value) ? value : (double?)null;
            }
        }
        public IReadOnlyDictionary<Guid, double> GetAll()
        {
            lock (_lock)
            {
                return new Dictionary<Guid, double>(_metrics);
            }
        }

        internal double GetSumOfChildrenInMs(TreeNode treeNode)
        {
            double sum = 0;
            lock (_lock)
            {
                foreach (var node in treeNode.Children)
                    sum += (_metrics.TryGetValue(node.Id, out var value) ? value : 0d);
            }
            return sum;
        }

        internal double GetSelfMsByNode(TreeNode treeNode)
        {
            double totalMs = 0;
            double childrenMs = 0;
             
            lock (_lock)
            {
                _metrics.TryGetValue(treeNode.Id, out totalMs);

                foreach (var node in treeNode.Children)
                    childrenMs += (_metrics.TryGetValue(node.Id, out var innerChildrenMs) ? innerChildrenMs : 0d);
            }

            var selfMs = totalMs;
            if (childrenMs > 0 && totalMs >= childrenMs)
                selfMs = totalMs - childrenMs;

            return selfMs;
        }
    }
}