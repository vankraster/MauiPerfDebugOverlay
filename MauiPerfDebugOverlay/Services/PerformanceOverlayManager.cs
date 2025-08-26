using MauiPerfDebugOverlay.Controls;
using MauiPerfDebugOverlay.Models;
using MauiPerfDebugOverlay.Utils;
using Microsoft.Maui.Layouts;

namespace MauiPerfDebugOverlay.Services
{
    public class PerformanceOverlayManager
    {
        private static PerformanceOverlayManager? _instance;
        private PerformanceOverlayView? _overlay;

        private PerformanceOverlayOptions _options;

        public static PerformanceOverlayManager Instance =>
            _instance ??= new PerformanceOverlayManager();

        private PerformanceOverlayManager() { }

        /// <summary>
        /// Activează overlay-ul global.
        /// Trebuie apelat o singură dată în MauiProgram.cs
        /// </summary>
        public void Enable(PerformanceOverlayOptions options)
        {
            _options = options;
            Application.Current.PageAppearing += OnPageAppearing;
        }

        private void OnPageAppearing(object? sender, Page page)
        {
            if (page is not ContentPage contentPage || contentPage.Content == null)
                return;

            // Dacă contentul nu e AbsoluteLayout, împachetează-l
            AbsoluteLayout abs;
            if (contentPage.Content is AbsoluteLayout existingLayout)
            {
                abs = existingLayout;
            }
            else
            {
                abs = new AbsoluteLayout();

                var originalContent = contentPage.Content;
                contentPage.Content = null; // eliminăm temporar
                AbsoluteLayout.SetLayoutFlags(originalContent, AbsoluteLayoutFlags.All);
                AbsoluteLayout.SetLayoutBounds(originalContent, new Rect(0, 0, 1, 1));
                abs.Children.Add(originalContent);

                contentPage.Content = abs;
            }

            // Verifică dacă overlay-ul există deja pe pagină
            if (!abs.Children.OfType<PerformanceOverlayView>().Any())
            {
                var overlay = new PerformanceOverlayView();
                AbsoluteLayout.SetLayoutFlags(overlay, AbsoluteLayoutFlags.None);
                AbsoluteLayout.SetLayoutBounds(overlay, new Rect(0, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
                abs.Children.Add(overlay);
                overlay.Start(_options);
            }
        } 
    }
}
