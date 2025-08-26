//using Foundation;
//using MauiPerfDebugOverlay.Interfaces;

//namespace MauiPerfDebugOverlay.Platforms
//{
//    public class IosNetworkProfiler : INetworkProfiler
//    {
//        public bool IsEnabled { get; set; } = true;
//        private double _requestsPerSecond = 0;
//        private double _bytesPerSecond = 0;

//        private int _requestCount = 0;
//        private long _byteCount = 0;

//        public double RequestsPerSecond => _requestsPerSecond;
//        public double BytesPerSecond => _bytesPerSecond;

//        public double TotalBytes => throw new NotImplementedException();

//        public double TotalRequests => throw new NotImplementedException();

//        public void UpdateMetrics()
//        {
//            _requestsPerSecond = _requestCount;
//            _bytesPerSecond = _byteCount;
//            _requestCount = 0;
//            _byteCount = 0;
//        }

//        public void RegisterProtocol()
//        {
//            NSUrlProtocol.RegisterClass(new NetworkInterceptor(this));
//        }

//        private class NetworkInterceptor : NSUrlProtocol
//        {
//            private readonly IosNetworkProfiler _profiler;

//            public NetworkInterceptor(IosNetworkProfiler profiler) => _profiler = profiler;

//            public override void StartLoading()
//            {
//                _profiler._requestCount++;
//                var dataLength = Request.Body?.Length ?? 0;
//                _profiler._byteCount += dataLength;
//                Client?.UrlProtocolDidFinishLoading(this);
//            }

//            public override void StopLoading() { }
//            public override bool CanInitWithRequest(NSUrlRequest request) => true;
//        }
//    }
//}
