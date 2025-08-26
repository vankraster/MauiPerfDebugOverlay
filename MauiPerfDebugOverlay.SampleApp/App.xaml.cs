using MauiPerfDebugOverlay.Services;

namespace MauiPerfDebugOverlay.SampleApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Activează PerformanceOverlay global
            PerformanceOverlayManager.Instance.Enable(new Models.PerformanceOverlayOptions
            {
                ShowBatteryUsage = true,
                ShowNetworkStats = true
            });

            MainPage = new AppShell();
        }
    }
}
