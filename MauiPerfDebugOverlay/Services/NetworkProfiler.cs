using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MauiPerfDebugOverlay.Services
{
    internal class NetworkProfiler
    {
        private static NetworkProfiler? _instance;
        public static NetworkProfiler Instance => _instance ??= new NetworkProfiler();

        private int _requestCount = 0;
        private long _bytesSent = 0;
        private long _bytesReceived = 0;
        private long _totalRequestTimeMs = 0;

        private int _requestsPerSecond = 0;
        private long _bytesSentPerSecond = 0;
        private long _bytesReceivedPerSecond = 0;

        public double RequestsPerSecond => _requestsPerSecond;
        public double BytesSentPerSecond => _bytesSentPerSecond;
        public double BytesReceivedPerSecond => _bytesReceivedPerSecond;
        public long TotalRequests => _requestCount;
        public long TotalBytesSent => _bytesSent;
        public long TotalBytesReceived => _bytesReceived;
        public double AverageRequestTimeMs => _requestCount > 0 ? _totalRequestTimeMs / _requestCount : 0;

        private NetworkProfiler()
        {
            // timer pentru update per secunda
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                _requestsPerSecond = _requestCount;
                _bytesSentPerSecond = _bytesSent;
                _bytesReceivedPerSecond = _bytesReceived;

                // reset pentru urmatoarea secunda
                _requestCount = 0;
                _bytesSent = 0;
                _bytesReceived = 0;

                return true;
            });
        }

        public HttpMessageHandler CreateHandler(HttpMessageHandler innerHandler)
        {
            return new ProfilingHandler(innerHandler, this);
        }




        public void Record(HttpWebRequest request, WebResponse response, TimeSpan elapsed)
        {
            long bytesSent = request.ContentLength > 0 ? request.ContentLength : 0;
            long bytesReceived = response.ContentLength > 0 ? response.ContentLength : 0;

            Interlocked.Add(ref _bytesSent, bytesSent);
            Interlocked.Add(ref _bytesReceived, bytesReceived);
            Interlocked.Add(ref _totalRequestTimeMs, (long)elapsed.TotalMilliseconds);
            Interlocked.Increment(ref _requestCount);
        }

        internal void Record(long bytesSent, long bytesReceived, TimeSpan elapsed)
        {
            Interlocked.Add(ref _bytesSent, bytesSent);
            Interlocked.Add(ref _bytesReceived, bytesReceived);
            Interlocked.Add(ref _totalRequestTimeMs, (long)elapsed.TotalMilliseconds);
            Interlocked.Increment(ref _requestCount);
        }

        private class ProfilingHandler : DelegatingHandler
        {
            private readonly NetworkProfiler _profiler;

            public ProfilingHandler(HttpMessageHandler inner, NetworkProfiler profiler) : base(inner)
            {
                _profiler = profiler;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                long requestSize = request.Content != null ? (await request.Content.ReadAsByteArrayAsync()).LongLength : 0;

                var sw = Stopwatch.StartNew();
                var response = await base.SendAsync(request, cancellationToken);
                sw.Stop();

                long responseSize = response.Content != null ? (await response.Content.ReadAsByteArrayAsync()).LongLength : 0;

                Interlocked.Increment(ref _profiler._requestCount);
                Interlocked.Add(ref _profiler._bytesSent, requestSize);
                Interlocked.Add(ref _profiler._bytesReceived, responseSize);
                Interlocked.Add(ref _profiler._totalRequestTimeMs, (long)sw.Elapsed.TotalMilliseconds);

                return response;
            }
        }
    }
}
