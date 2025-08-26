using System.Net.Http;
using System.Reflection;

namespace MauiPerfDebugOverlay.Services
{  
    public static class HttpClientInterceptor
    {
        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized) return;

            var httpClientType = typeof(HttpClient);
            var innerHandlerField = httpClientType
                .GetField("_handler", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? httpClientType.GetField("handler", BindingFlags.Instance | BindingFlags.NonPublic);

            if (innerHandlerField == null)
            {
                // Nu s-a găsit câmpul -> skip
#if DEBUG
                System.Diagnostics.Debug.WriteLine("⚠️ HttpClient handler field not found. Interception skipped.");
#endif
                return;
            }

            _initialized = true;

            // Cream un client de test doar pentru a putea "fura" handler-ul
            using var client = new HttpClient();
            var origHandler = innerHandlerField.GetValue(client) as HttpMessageHandler;

            if (origHandler != null)
            {
                var wrapped = NetworkProfiler.Instance.CreateHandler(origHandler);
                innerHandlerField.SetValue(client, wrapped);

#if DEBUG
                System.Diagnostics.Debug.WriteLine("✅ HttpClient interception active.");
#endif
            }
        }
    }

}