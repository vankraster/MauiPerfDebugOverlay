using MauiPerfDebugOverlay.Models.Internal;
using MauiPerfDebugOverlay.Utils;
using System.Globalization;
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
                                        {CultureInfo.CurrentCulture.GetCultureDetailsForAI()}
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

        internal async void AskForNetworkMetrics(List<NetworkMetric> selectedData)
        {
            if (selectedData == null || selectedData.Count == 0)
                return;

            LastAnalyzerResponse = "Waiting the response from Gemini while analyzing selected network metrics...";
            ResponseChanged?.Invoke(LastAnalyzerResponse);

            // Serializăm datele rețelei într-un format text clar
            var serialized = string.Join("\n", selectedData.Select(m =>
            {
                string tags = (m.Tags != null && m.Tags.Length > 0)
                    ? string.Join(", ", m.Tags.Select(t => $"{t.Key}={t.Value}"))
                    : "no-tags";

                return $"[{m.Timestamp:HH:mm:ss.fff}] {m.Name} = {m.Value} (Tags: {tags})";
            }));

            string networkPrompt = $@"I have collected some .NET runtime network metrics.
                                            {CultureInfo.CurrentCulture.GetCultureDetailsForAI()}
                                    Please analyze these metrics for potential **real issues**, following these strict rules:

                                    1. Performance: Identify only CLEAR network performance problems (e.g., DNS resolution delays, repeated failures, unusually long response times, high error rates).
                                    2. Patterns: Highlight only OBSERVABLE patterns or anomalies (e.g., spikes, repetitive retries, abnormal values).
                                    3. Avoid guessing or inventing causes that are not visible in the data. Do not give generic networking tips.
                                    4. If no real problems are evident, explicitly state: ""No clear issues detected"".
                                    5. Answer briefly and in a structured format (Performance, Patterns, Other).

                                    Here are the collected metrics: 
                                    {serialized}";

            LastAnalyzerResponse = await GetResponseAsync(networkPrompt);
            ResponseChanged?.Invoke(LastAnalyzerResponse);
        }

    }
}
