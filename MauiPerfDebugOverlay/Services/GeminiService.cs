using System.Text.Json;

namespace MauiPerfDebugOverlay.Services
{
    internal class GeminiService
    {
        private static GeminiService? _instance;
        public static GeminiService Instance => _instance ??= new GeminiService();


        private readonly HttpClient _httpClient;

        private GeminiService()
        {
            _httpClient = new HttpClient();
        }


        private string treeAnalyzerResponse;


        private async Task<string> GetTreeAnalyzerResponseAsync()
        {
             
            if (string.IsNullOrWhiteSpace(treeAnalyzerResponse))
            {
                string treePrompt = "";
                treeAnalyzerResponse = await GetResponseAsync(treePrompt);
            }

            return treeAnalyzerResponse;
        }


        private async Task<string> GetResponseAsync(string prompt)
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = prompt
                            }
                        }
                    }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post,
                    "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent");

            request.Headers.Add("X-goog-api-key", MauiPerfDebugOverlay.Extensions.PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.GeminiAPIKey);

            request.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody),
                                                System.Text.Encoding.UTF8,
                                                "application/json");


            var response = await _httpClient.SendAsync(request);


            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                // Parse the response JSON to extract the generated text
                using (var document = JsonDocument.Parse(responseContent))
                {
                    var textResponse = document.RootElement
                            .GetProperty("candidates")[0]
                            .GetProperty("content")
                            .GetProperty("parts")[0]
                            .GetProperty("text")
                            .GetString();

                    return textResponse ?? "No response text found.";
                }
            }
            else
            {
                return $"Error: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}";
            }

        }
    }
}
