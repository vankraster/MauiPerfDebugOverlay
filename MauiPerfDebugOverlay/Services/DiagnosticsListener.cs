using System.Diagnostics.Metrics;

namespace MauiPerfDebugOverlay.Services
{
    public class DiagnosticsListener : IDisposable
    {  
        private static readonly Lazy<DiagnosticsListener> _instance = new(() => new DiagnosticsListener());
        public static DiagnosticsListener Instance => _instance.Value;



        private readonly MeterListener _listener;
        private readonly System.Timers.Timer _observableTimer;

        private readonly Dictionary<string, object> _metrics = new();
        private readonly object _lock = new();

        /// <summary>
        /// Raised whenever the metrics collection changes.
        /// Arguments: action type ("Add" or "Clear"), metric key, metric value (if applicable).
        /// </summary>
        public event Action<string, string?, object?>? CollectionChanged;

        public DiagnosticsListener(double observableIntervalMs = 500)
        {
            _listener = new MeterListener();

            // Activează pentru toate instrumentele deja existente
            //foreach (var meter in Meter.GetAllMeters())
            //{
            //    foreach (var instrument in meter.Instruments)
            //    {
            //        _listener.EnableMeasurementEvents(instrument);
            //    }
            //}


            _listener.InstrumentPublished += (instrument, l) =>
            {
                l.EnableMeasurementEvents(instrument);
            };

            //byte
            _listener.SetMeasurementEventCallback<byte>((inst, value, tags, state) =>
               Store(inst.Name, value, tags));

            // int
            _listener.SetMeasurementEventCallback<int>((inst, value, tags, state) =>
                Store(inst.Name, value, tags));

            // float
            _listener.SetMeasurementEventCallback<float>((inst, value, tags, state) =>
                Store(inst.Name, value, tags));

            // double
            _listener.SetMeasurementEventCallback<double>((inst, value, tags, state) =>
                Store(inst.Name, value, tags));

            // decimal
            _listener.SetMeasurementEventCallback<decimal>((inst, value, tags, state) =>
                Store(inst.Name, value, tags));

            //short
            _listener.SetMeasurementEventCallback<short>((inst, value, tags, state) =>
               Store(inst.Name, value, tags));

            // long
            _listener.SetMeasurementEventCallback<long>((inst, value, tags, state) =>
                Store(inst.Name, value, tags));
             
            _listener.Start();

            _observableTimer = new System.Timers.Timer(observableIntervalMs);
            _observableTimer.Elapsed += (s, e) => _listener.RecordObservableInstruments();
            _observableTimer.Start();
        }

        private void Store(string metricName, object value, ReadOnlySpan<KeyValuePair<string, object?>> tags)
        {
            var tagKey = tags.Length > 0
                ? $"{metricName}:{string.Join(",", tags.ToArray().Select(t => $"{t.Key}={t.Value}"))}"
                : metricName;

            lock (_lock)
            {
                _metrics[tagKey] = value;
            }

            CollectionChanged?.Invoke("Add", tagKey, value);
        }

        public object? GetValue(string key)
        {
            lock (_lock)
            {
                return _metrics.TryGetValue(key, out var value) ? value : null;
            }
        }

        public IReadOnlyDictionary<string, object> GetAll()
        {
            lock (_lock)
            {
                return new Dictionary<string, object>(_metrics);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _metrics.Clear();
            }

            CollectionChanged?.Invoke("Clear", null, null);
        }

        public void Dispose()
        {
            _observableTimer?.Stop();
            _observableTimer?.Dispose();
            _listener?.Dispose();
        }

        public int Count()
        {
            return _metrics.Count();
        }
    }
}
