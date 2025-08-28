using Microsoft.Extensions.DependencyInjection;

namespace MauiPerfDebugOverlay.SampleApp
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        private HttpClient _httpClient => Handler.MauiContext.Services.GetRequiredService<HttpClient>();

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);

            try
            {


                // Important: Google cere un User-Agent, altfel poate returna 403
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (MAUI Test App)");

                string url = "https://www.google.ro/search?q=iasi";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string html = await response.Content.ReadAsStringAsync();
                    // doar ca test, afișăm primele caractere
                    await DisplayAlert("Success", html.Substring(0, Math.Min(200, html.Length)), "OK");
                }
                else
                {
                    await DisplayAlert("Error", $"Status: {response.StatusCode}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Exception", ex.Message, "OK");
            }
        }

    }

}
