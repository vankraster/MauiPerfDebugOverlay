using MauiPerfDebugOverlay.Interfaces;
using System.Net.Http;
using System.Diagnostics;

namespace MauiPerfDebugOverlay.Platforms.Windows
{
    public class WindowsNetworkProfiler : INetworkProfiler
    {
        public bool IsEnabled { get; set; } = true;
        private double _requestsPerSecond = 0;
        private double _bytesPerSecond = 0;

        public double RequestsPerSecond => _requestsPerSecond;
        public double BytesPerSecond => _bytesPerSecond;

        public double TotalBytes => throw new NotImplementedException();

        public double TotalRequests => throw new NotImplementedException();

        private int _requestCount = 0;
        private long _byteCount = 0;

        public void UpdateMetrics()
        {
            _requestsPerSecond = _requestCount;
            _bytesPerSecond = _byteCount;
            _requestCount = 0;
            _byteCount = 0;
        }

        public void RegisterHttpClient(HttpClient client)
        {
            var handler = new ProfilingHandler(this)
            {
                InnerHandler = client.Handler
            };
            client = new HttpClient(handler);
        }

        private class ProfilingHandler : DelegatingHandler
        {
            private readonly WindowsNetworkProfiler _profiler;

            public ProfilingHandler(WindowsNetworkProfiler profiler)
            {
                _profiler = profiler;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = await base.SendAsync(request, cancellationToken);
                _profiler._requestCount++;
                if (response.Content != null)
                {
                    var bytes = response.Content.Headers.ContentLength ?? 0;
                    _profiler._byteCount += bytes;
                }
                return response;
            }
        }
    }
} 