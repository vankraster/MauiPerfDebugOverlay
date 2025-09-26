using MauiPerfDebugOverlay.Models.Internal;
using System.Diagnostics.Metrics;

namespace MauiPerfDebugOverlay.Services
{
    public class DiagnosticsListener : IDisposable
    {
        private static readonly Lazy<DiagnosticsListener> _instance = new(() => new DiagnosticsListener());
        public static DiagnosticsListener Instance => _instance.Value;

        private readonly MeterListener _listener;
        private readonly System.Timers.Timer _observableTimer;

        private readonly List<NetworkMetric> _networkMetrics = new();
        private readonly object _lockNetwork = new();

        private readonly Dictionary<string, object> _metricsExceptions = new();
        private readonly object _lockExceptions = new();

        private readonly Dictionary<string, object> _metrics = new();
        private readonly object _lock = new();

        private static readonly HashSet<string> _ignoredHosts = new(StringComparer.OrdinalIgnoreCase)
        {
            "api.nuget.org",
            "generativelanguage.googleapis.com"
        };

        /// <summary>
        /// Raised whenever the metrics collection changes.
        /// Arguments: action type ("Add" or "Clear"), metric key, metric value (if applicable).
        /// </summary>
        public event Action<string, string?, object?>? CollectionChanged;

        public event Action<string, string?, object?>? CollectionNetworkChanged;

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
            if (metricName.StartsWith("dotnet.exceptions"))
            {
                if (tags.Length == 0)
                {
                    lock (_lockExceptions)
                        _metricsExceptions[metricName] = value;

                    CollectionChanged?.Invoke("Add", metricName, value);
                    return;
                }

                foreach (var tag in tags)
                {
                    //var tagKey = $"{metricName}:{tag.Key}={tag.Value}";

                    lock (_lockExceptions)
                        _metricsExceptions[tag.Value?.ToString() ?? "NO TAG VALUE"] = value;

                    CollectionChanged?.Invoke("Add", tag.Key, value);
                }
            }
            else if (metricName.StartsWith("http.client") || metricName.StartsWith("dns"))
            {
                // verificăm tag-urile înainte să adăugăm metrica
                if (ShouldIgnoreRequest(tags))
                    return;

                // Stochează și în lista de rețea pentru analiză ulterioară
                lock (_lockNetwork)
                {
                    _networkMetrics.Insert(0, new NetworkMetric
                    {
                        Name = metricName,
                        Value = value,
                        Tags = tags.ToArray(),
                        Timestamp = DateTime.Now
                    });

                    // Păstrează doar ultimele 100 de intrări
                    if (_networkMetrics.Count > 100)
                    {
                        _networkMetrics.RemoveAt(_networkMetrics.Count - 1);
                    }
                }

                CollectionNetworkChanged?.Invoke("Add", metricName, value);
            }
            else
            {
                if (tags.Length == 0)
                {
                    lock (_lock)
                        _metrics[metricName] = value;

                    CollectionChanged?.Invoke("Add", metricName, value);
                    return;
                }

                foreach (var tag in tags)
                {
                    var tagKey = $"{metricName}:{tag.Key}={tag.Value}";

                    lock (_lock)
                        _metrics[tagKey] = value;

                    CollectionChanged?.Invoke("Add", tagKey, value);
                }
            }
        }

        private bool ShouldIgnoreRequest(ReadOnlySpan<KeyValuePair<string, object?>> tags)
        {
            foreach (var tag in tags)
            {
                if (tag.Value is string value)
                {
                    // verificăm dacă vreun host din listă apare în valoare
                    foreach (var ignored in _ignoredHosts)
                    {
                        if (value.Contains(ignored, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                }
            }
             
            //foreach (var tag in tags)
            //{
            //    if (tag.Key == "http.url" && tag.Value is string url)
            //    {
            //        // Ignoră toate request-urile către nuget.org
            //        if (url.Contains("api.nuget.org", StringComparison.OrdinalIgnoreCase))
            //            return true;
            //    }

            //    if (tag.Key == "server.address" && tag.Value is string host)
            //    {
            //        if (host.Equals("api.nuget.org", StringComparison.OrdinalIgnoreCase))
            //            return true;
            //    }

            //    if (tag.Key == "dns.question.name" && tag.Value is string dns)
            //    {
            //        if (dns.Equals("api.nuget.org", StringComparison.OrdinalIgnoreCase))
            //            return true;
            //    }
            //}

            return false;
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

        public IReadOnlyDictionary<string, object> GetAllExceptions()
        {
            lock (_lockExceptions)
            {
                return new Dictionary<string, object>(_metricsExceptions);
            }
        }

        public IReadOnlyList<NetworkMetric> GetAllNetwork()
        {
            lock (_lockNetwork)
            {
                return new List<NetworkMetric>(_networkMetrics);
            }
        }

        public void CollapseExpandNetwork(int key)
        {
            lock (_lockNetwork)
            {
                var metric = _networkMetrics.FirstOrDefault(m => m.Id == key);
                if (metric != null)
                {
                    metric.IsExpanded = !metric.IsExpanded;
                }
            }
        }


        public void Dispose()
        {
            _observableTimer?.Stop();
            _observableTimer?.Dispose();
            _listener?.Dispose();
        }

        public int Count()
        {
            return (_metrics.Count() > 0 ? _metrics.Count() + 1 : 0) +
                   (_metricsExceptions.Count() > 0 ? _metricsExceptions.Count() + 1 : 0);
        }



        internal double NewHeight()
        {
            double height = 0;
            lock (_lockNetwork)
            {
                if (_networkMetrics.Count == 0)
                    return 36;

                int count = _networkMetrics.Count + 1; // +1 pentru titlu/section header

                height = count * 36;

                // adaugă numărul de tag-uri pentru fiecare metrică expandată
                int countTags = _networkMetrics
                      .Where(m => m.IsExpanded && m.Tags != null)
                      .Sum(m => m.Tags.Length);


                height += 36 * countTags;
            }

            return height + 20;
        }
    }
}
