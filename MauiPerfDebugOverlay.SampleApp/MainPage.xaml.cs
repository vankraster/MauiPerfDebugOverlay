using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.Metrics;

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

        private void CustomMetrics_Clicked(object sender, EventArgs e)
        {
            //Full documentation on how to use custom metrics in .NET:
            //https://learn.microsoft.com/en-us/dotnet/core/diagnostics/metrics-instrumentation


            Meter s_meter = new Meter("HatCo.Store");
            Counter<int> s_orderCounter = s_meter.CreateCounter<int>("orders");
            Counter<int> s_hatsSold = s_meter.CreateCounter<int>("hatco.store.hats_sold");



            s_orderCounter.Add(1);
            s_hatsSold.Add(2,
                        new KeyValuePair<string, object?>("product.color", "red"),
                        new KeyValuePair<string, object?>("product.size", 12));

            s_meter.CreateObservableGauge<int>("hatco.store.orders_pending", () => new Measurement<int>[]
        {
            // pretend these measurements were read from a real queue somewhere
            new Measurement<int>(6, new KeyValuePair<string,object?>("customer.country", "Italy")),
            new Measurement<int>(3, new KeyValuePair<string,object?>("customer.country", "Spain")),
            new Measurement<int>(1, new KeyValuePair<string,object?>("customer.country", "Mexico")),
        });

        }
    }

}
