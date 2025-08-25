using MauiPerfDebugOverlay.Controls;
using MauiPerfDebugOverlay.Utils;
using Microsoft.Maui.Layouts;

namespace MauiPerfDebugOverlay.Services
{
    public class PerformanceOverlayManager
    {
        private static PerformanceOverlayManager? _instance;
        private PerformanceOverlayView? _overlay;

        public static PerformanceOverlayManager Instance =>
            _instance ??= new PerformanceOverlayManager();

        private PerformanceOverlayManager() { }

        /// <summary>
        /// Activează overlay-ul global.
        /// Trebuie apelat o singură dată în MauiProgram.cs
        /// </summary>
        public void Enable()
        {
            Application.Current.PageAppearing += OnPageAppearing;
        }

        private void OnPageAppearing(object? sender, Page page)
        {
            if (page is ContentPage contentPage && contentPage.Content != null)
            {
                EnsureOverlayCreated();

                // Dacă contentul paginii nu e deja AbsoluteLayout
                if (contentPage.Content is not AbsoluteLayout abs)
                {
                    abs = new AbsoluteLayout();

                    // Adaugă conținutul existent full screen
                    AbsoluteLayout.SetLayoutFlags(contentPage.Content, AbsoluteLayoutFlags.All);
                    AbsoluteLayout.SetLayoutBounds(contentPage.Content, new Rect(0, 0, 1, 1));
                    abs.Children.Add(contentPage.Content);
                    AddOverlayToLayout(abs);

                    contentPage.Content = abs;
                }
                else
                    AddOverlayToLayout(abs);

            }
        }

        private void EnsureOverlayCreated()
        {
            if (_overlay == null || _overlay?.Parent == null)
            {
                _overlay = new PerformanceOverlayView();
            }
        }

        private void AddOverlayToLayout(AbsoluteLayout layout)
        {
            // Elimină instanțele vechi ale overlay-ului
            foreach (var child in layout.Children.OfType<PerformanceOverlayView>().ToList())
                layout.Children.Remove(child);

            // Adaugă overlay-ul deasupra
            AbsoluteLayout.SetLayoutFlags(_overlay!, AbsoluteLayoutFlags.PositionProportional);
            AbsoluteLayout.SetLayoutBounds(_overlay!, new Rect(0, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));

            layout.Children.Add(_overlay!);
            _overlay!.Start();
        }
    }
}
