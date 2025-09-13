using System.Diagnostics.Metrics;

namespace MauiPerfDebugOverlay.Services
{
    public class DiagnosticsListener : IDisposable
    {
        private readonly MeterListener _listener;
        private readonly System.Timers.Timer _observableTimer;

        // Evenimente generice pentru orice metrică
        public event Action<string, object, Dictionary<string, string>> OnMeasurement;

        public DiagnosticsListener(double observableIntervalMs = 500)
        {
            _listener = new MeterListener();

            // Publicarea instrumentelor
            _listener.InstrumentPublished += (instrument, l) =>
            {
                // Enable pentru toate instrumentele
                l.EnableMeasurementEvents(instrument);
            };

            // Callback pentru int
            _listener.SetMeasurementEventCallback<int>((inst, value, tags, state) =>
            {
                var tagDict = tags.ToArray().ToDictionary(t => t.Key, t => t.Value?.ToString() ?? "");
                OnMeasurement?.Invoke(inst.Name, value, tagDict);
            });

            // Callback pentru double
            _listener.SetMeasurementEventCallback<double>((inst, value, tags, state) =>
            {
                var tagDict = tags.ToArray().ToDictionary(t => t.Key, t => t.Value?.ToString() ?? "");
                OnMeasurement?.Invoke(inst.Name, value, tagDict);
            });

            // Optional: suport pentru long / float / decimal în viitor
            _listener.SetMeasurementEventCallback<long>((inst, value, tags, state) =>
            {
                var tagDict = tags.ToArray().ToDictionary(t => t.Key, t => t.Value?.ToString() ?? "");
                OnMeasurement?.Invoke(inst.Name, value, tagDict);
            });

            _listener.Start();

            // Timer pentru gauge-uri observabile
            _observableTimer = new System.Timers.Timer(observableIntervalMs);
            _observableTimer.Elapsed += (s, e) => _listener.RecordObservableInstruments();
            _observableTimer.Start();
        }

        public void Dispose()
        {
            _observableTimer?.Stop();
            _observableTimer?.Dispose();
            _listener?.Dispose();
        }
    }
}
