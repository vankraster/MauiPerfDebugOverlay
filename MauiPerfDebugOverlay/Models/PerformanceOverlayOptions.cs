namespace MauiPerfDebugOverlay.Models
{
    public class PerformanceOverlayOptions
    {
        public bool ShowBatteryUsage { get; set; } = false;
        public bool ShowNetworkStats { get; set; } = false;
        public bool ShowDiskIO { get; set; } = false;
    }
}
