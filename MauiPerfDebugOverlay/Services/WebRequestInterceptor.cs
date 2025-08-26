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

    //public class ProfilingHttpWebRequest : WebRequest
    //{
    //    private readonly HttpWebRequest _inner;

    //    public ProfilingHttpWebRequest(HttpWebRequest inner)
    //    {
    //        _inner = inner;
    //    }

    //    public override string Method { get => _inner.Method; set => _inner.Method = value; }
    //    public override Uri RequestUri => _inner.RequestUri;

    //    public override WebResponse GetResponse()
    //    {
    //        var sw = Stopwatch.StartNew();
    //        var resp = _inner.GetResponse();
    //        sw.Stop();
    //        NetworkProfiler.Instance.Record(_inner, resp, sw.Elapsed);
    //        return resp;
    //    }

    //    public override async Task<WebResponse> GetResponseAsync()
    //    {
    //        var sw = Stopwatch.StartNew();
    //        var resp = await _inner.GetResponseAsync();
    //        sw.Stop();
    //        NetworkProfiler.Instance.Record(_inner, resp, sw.Elapsed);
    //        return resp;
    //    }
         
    //}

    public class ProfilingHttpWebRequest : WebRequest
    {
        private readonly HttpWebRequest _inner;

        public ProfilingHttpWebRequest(WebRequest inner)
        {
            if (inner is not HttpWebRequest httpReq)
                throw new ArgumentException("inner must be HttpWebRequest", nameof(inner));

            _inner = httpReq;
        }

        // Forward essential properties
        public override string Method { get => _inner.Method; set => _inner.Method = value; }
        public override Uri RequestUri => _inner.RequestUri;
        public override string ContentType { get => _inner.ContentType; set => _inner.ContentType = value; }
        public override WebHeaderCollection Headers { get => _inner.Headers; set => _inner.Headers = value; }
        public override long ContentLength { get => _inner.ContentLength; set => _inner.ContentLength = value; }

        public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
            => _inner.BeginGetResponse(callback, state);

        public override WebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var resp = _inner.EndGetResponse(asyncResult);
                return resp;
            }
            finally
            {
                sw.Stop();
                RecordMetrics(sw.Elapsed);
            }
        }

        public override WebResponse GetResponse()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var resp = _inner.GetResponse();
                return resp;
            }
            finally
            {
                sw.Stop();
                RecordMetrics(sw.Elapsed);
            }
        }

        public override async Task<WebResponse> GetResponseAsync()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var resp = await _inner.GetResponseAsync();
                return resp;
            }
            finally
            {
                sw.Stop();
                RecordMetrics(sw.Elapsed);
            }
        }

        public override Stream GetRequestStream()
        {
            return _inner.GetRequestStream();
        }

        public override async Task<Stream> GetRequestStreamAsync()
        {
            return await _inner.GetRequestStreamAsync();
        }

        private void RecordMetrics(TimeSpan elapsed)
        {
            try
            {
                long bytesSent = _inner.ContentLength;
                long bytesReceived = 0;

                if (_inner.GetResponse() is HttpWebResponse resp)
                {
                    bytesReceived = resp.ContentLength;
                }

                NetworkProfiler.Instance.Record(bytesSent, bytesReceived, elapsed);
            }
            catch
            {
                // swallow exceptions, do not break request
            }
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
            WebRequest.RegisterPrefix("http://", profilingCreator);
            WebRequest.RegisterPrefix("https://", profilingCreator);
        }
    }
}
