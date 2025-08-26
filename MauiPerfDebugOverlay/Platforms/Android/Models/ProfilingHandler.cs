namespace MauiPerfDebugOverlay.Platforms.Android.Models
{
    public class ProfilingHandler : DelegatingHandler
    {
        private readonly NetworkProfiler _profiler;

        public ProfilingHandler(HttpMessageHandler inner, NetworkProfiler profiler)
            : base(inner)
        {
            _profiler = profiler;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            _profiler._requestCount++;

            if (response.Content != null)
            {
                _profiler._byteCount += response.Content.Headers.ContentLength ?? 0;
            }

            return response;
        }
    }
}
