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
    }
}
