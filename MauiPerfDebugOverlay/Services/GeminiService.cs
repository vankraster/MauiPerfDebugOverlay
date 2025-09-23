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

            string treePrompt = $@" I have a XAML subtree from a .NET MAUI page.

                                    Please analyze this subtree for potential issues, but follow these rules:

                                    1. Performance: Identify only CLEAR performance bottlenecks based on SelfMs/HandlerChanged timings.
                                    2. Structural/Layout: Identify only CLEAR structural/layout problems, e.g., excessive nesting, inappropriate layout usage, or patterns that could degrade performance or maintainability.
                                    3. If there are no real issues in a category, just say ""No clear issues detected"" for that category.
                                    4. Avoid inventing problems or giving generic optimization tips. Only mention real, observable concerns.
                                    5. Give a brief, structured answer.

                                    Subtree details:
                                    {serializedTree} ";

            LastAnalyzerResponse = await GetResponseAsync(treePrompt);
            ResponseChanged?.Invoke(LastAnalyzerResponse);
        }

    }
}
