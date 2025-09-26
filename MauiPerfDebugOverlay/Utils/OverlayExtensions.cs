using MauiPerfDebugOverlay.Controls;
using System.Globalization;

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


        public static string FormatTime(this double ms)
        {

            if (ms < 0)
                return "0ms";

            return ms >= 1000
                ? $"{ms / 1000:F3}s"
                : $"{ms:F4}ms";
        }

        public static string GetCultureDetailsForAI(this CultureInfo? culture)
        {
            return $"⚠️ Numbers are formatted using the '{culture.Name}' culture " +
                   $"({culture.EnglishName}), where the decimal separator is " +
                   $"'{culture.NumberFormat.NumberDecimalSeparator}' and the thousands separator is " +
                   $"'{culture.NumberFormat.NumberGroupSeparator}'.";
        }
    }
}
