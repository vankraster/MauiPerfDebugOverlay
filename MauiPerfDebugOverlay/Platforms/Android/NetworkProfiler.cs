
using MauiPerfDebugOverlay.Interfaces;
using MauiPerfDebugOverlay.Platforms.Android.Models;

namespace MauiPerfDebugOverlay.Platforms
{
    public class NetworkProfiler : INetworkProfiler
    {
        public bool IsEnabled { get; set; } = true;
        private double _requestsPerSecond = 0;
        private double _bytesPerSecond = 0;

        internal int _requestCount = 0;
        internal long _byteCount = 0;

        public double RequestsPerSecond => _requestsPerSecond;
        public double BytesPerSecond => _bytesPerSecond;

        public double TotalBytes => throw new NotImplementedException();

        public double TotalRequests => throw new NotImplementedException();

        public void UpdateMetrics()
        {
            _requestsPerSecond = _requestCount;
            _bytesPerSecond = _byteCount;
            _requestCount = 0;
            _byteCount = 0;
        }

        public HttpMessageHandler CreateHandler(HttpMessageHandler inner)
        {
            return new ProfilingHandler(inner, this);
        } 
    }
} 