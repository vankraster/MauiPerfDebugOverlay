using MauiPerfDebugOverlay.Extensions;
using MauiPerfDebugOverlay.Services;
using Microsoft.Extensions.Logging;

namespace MauiPerfDebugOverlay.SampleApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UsePerformanceDebugOverlay(new Models.PerformanceOverlayOptions
                {
                    ShowBatteryUsage = true,
                    ShowNetworkStats = true,
                    ShowAlloc_GC = true,
                    ShowCPU_Usage = true,
                    ShowFrame = true,
                    ShowMemory = true,
                    ShowLoadTime = true,
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
