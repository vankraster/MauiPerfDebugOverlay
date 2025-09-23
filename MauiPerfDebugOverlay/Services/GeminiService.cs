using MauiPerfDebugOverlay.Models.Internal;
using System.Text.Json;

namespace MauiPerfDebugOverlay.Services
{
    internal class GeminiService
    {
        private static GeminiService? _instance;
        public static GeminiService Instance => _instance ??= new GeminiService();

        public event Action<string>? ResponseChanged;

        private readonly HttpClient _httpClient;
        public string LastAnalyzerResponse { private set; get; }
        private GeminiService()
        {
            _httpClient = new HttpClient();
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


        internal async Task AskForTreeNode(TreeNode clickedNode)
        {
            LastAnalyzerResponse = "Waiting the response from Gemini while analyzing node: " + clickedNode.Name;
            ResponseChanged?.Invoke(LastAnalyzerResponse);
            string serializedTree = TreeNode.SerializeTree(clickedNode);

            string treePrompt = $@" I have a XAML structure from a .NET MAUI page.

                                Please analyze the following subtree and provide:  
                                1. Any possible performance issues based on SelfMs timings.
                                2. Suggestions for optimization.

                                Subtree details:
                                {serializedTree}

                                Answer in a clear, structured and brief way.";

            LastAnalyzerResponse = await GetResponseAsync(treePrompt);
            ResponseChanged?.Invoke(LastAnalyzerResponse);
        }

    }
}
