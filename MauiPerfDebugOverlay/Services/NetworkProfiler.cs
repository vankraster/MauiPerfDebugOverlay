using System.Diagnostics;

namespace MauiPerfDebugOverlay.Services
{
    internal class ProfilingHttpClient : HttpClient
    {
        public ProfilingHttpClient()
      : base(CreateProfilingHandler()) 
        {

        }
        private static HttpMessageHandler CreateProfilingHandler()
        {
            // handler-ul de profilare
            return new NetworkProfiler.NetworkProfilingHandler(new HttpClientHandler(), NetworkProfiler.Instance);
        }

    }

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

        //public double RequestsPerSecond => _requestsPerSecond;
        //public double BytesSentPerSecond => _bytesSentPerSecond;
        //public double BytesReceivedPerSecond => _bytesReceivedPerSecond;

        public long TotalRequests => _requestCount;
        public long TotalBytesSent => _bytesSent;
        public long TotalBytesReceived => _bytesReceived;
        public double AverageRequestTimeMs => _requestCount > 0 ? _totalRequestTimeMs / _requestCount : 0;

        private NetworkProfiler()
        {
            // timer pentru update per secunda
            //Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            //{   
            //    // reset pentru urmatoarea secunda
            //    _requestsPerSecond = 0;
            //    _bytesSentPerSecond = 0;
            //    _bytesReceivedPerSecond = 0;

            //    return true;
            //});
        }

        public HttpMessageHandler CreateHandler(HttpMessageHandler innerHandler)
        {
            return new NetworkProfilingHandler(innerHandler, this);
        }






        internal class NetworkProfilingHandler : DelegatingHandler
        {
            private readonly NetworkProfiler _profiler;

            public NetworkProfilingHandler(HttpMessageHandler inner, NetworkProfiler profiler) : base(inner)
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


                //Interlocked.Increment(ref _profiler._requestsPerSecond);
                //Interlocked.Add(ref _profiler._bytesSentPerSecond, requestSize);
                //Interlocked.Add(ref _profiler._bytesReceivedPerSecond, responseSize);


                Interlocked.Add(ref _profiler._totalRequestTimeMs, (long)sw.Elapsed.TotalMilliseconds);

                return response;
            }


        }

    }
}
