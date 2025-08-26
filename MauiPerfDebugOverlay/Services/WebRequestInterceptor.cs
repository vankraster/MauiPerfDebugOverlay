using System.Diagnostics;
using System.Net;

namespace MauiPerfDebugOverlay.Services
{
    public class DefaultWebRequestCreator : IWebRequestCreate
    {
        public WebRequest Create(Uri uri)
        {
            return WebRequest.CreateDefault(uri);
        }
    }

    public class ProfilingWebRequestCreator : IWebRequestCreate
    {
        private readonly IWebRequestCreate _inner;

        public ProfilingWebRequestCreator(IWebRequestCreate inner)
        {
            _inner = inner;
        }

        public WebRequest Create(Uri uri)
        {
            var req = _inner.Create(uri) as HttpWebRequest;
            return new ProfilingHttpWebRequest(req);
        } 
    }

    public class ProfilingHttpWebRequest : WebRequest
    {
        private readonly HttpWebRequest _inner;

        public ProfilingHttpWebRequest(HttpWebRequest inner)
        {
            _inner = inner;
        }

        public override string Method { get => _inner.Method; set => _inner.Method = value; }
        public override Uri RequestUri => _inner.RequestUri;

        public override WebResponse GetResponse()
        {
            var sw = Stopwatch.StartNew();
            var resp = _inner.GetResponse();
            sw.Stop();
            NetworkProfiler.Instance.Record(_inner, resp, sw.Elapsed);
            return resp;
        }

        public override async Task<WebResponse> GetResponseAsync()
        {
            var sw = Stopwatch.StartNew();
            var resp = await _inner.GetResponseAsync();
            sw.Stop();
            NetworkProfiler.Instance.Record(_inner, resp, sw.Elapsed);
            return resp;
        }
         
    }
 
    public class WebRequestInterceptor
    { 
        public static void Initialize()
        {
            // "creator" normal care știe să creeze HttpWebRequest fără profiling
            var defaultCreator = new DefaultWebRequestCreator();

            // îl împachetezi cu profiling
            var profilingCreator = new ProfilingWebRequestCreator(defaultCreator);

            // registrezi global interceptarea
            WebRequest.RegisterPrefix("http", profilingCreator);
            WebRequest.RegisterPrefix("https", profilingCreator);
        }
    }
}
