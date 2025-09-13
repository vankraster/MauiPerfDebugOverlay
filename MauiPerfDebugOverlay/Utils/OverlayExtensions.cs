using MauiPerfDebugOverlay.Controls;

namespace MauiPerfDebugOverlay.Utils
{
    internal static class OverlayExtensions
    {
        /// <summary>
        /// Verifică dacă overlay-ul există deja în layout
        /// </summary>
        public static bool IsVisibleInLayout(this PerformanceOverlayView overlay, Layout layout)
        {
            return layout.Children.Contains(overlay);
        }


        public static string FormatTime(this double? ms)
        {
            if (!ms.HasValue)
                return "-";

            if (ms < 0)
                return "0ms";

            return ms.Value >= 1000
                ? $"{ms.Value / 1000:F3}s"
                : $"{ms.Value:F4}ms";
        }
    }
}
