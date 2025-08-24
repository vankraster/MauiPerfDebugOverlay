using MauiPerfDebugOverlay.Controls;
using MauiPerfDebugOverlay.Utils;

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
            if (page is ContentPage contentPage)
            {
                EnsureOverlayCreated();

                if (contentPage.Content is Layout layout)
                {
                    AddOverlayToLayout(layout);
                }
                else if (contentPage.Content != null)
                {
                    // Dacă pagina nu are Layout, împachetează conținutul într-un Grid
                    var grid = new Grid();
                    grid.Children.Add(contentPage.Content);
                    grid.Children.Add(_overlay!);
                    contentPage.Content = grid;

                    _overlay!.Start();
                }
            }
        }

        private void EnsureOverlayCreated()
        {
            if (_overlay == null)
            {
                _overlay = new PerformanceOverlayView();
            }
        }

        private void AddOverlayToLayout(Layout layout)
        {
            if (_overlay?.Parent != null && _overlay.Parent != layout)
            {
                // s-a "pierdut" în alt părinte dispărut → recrează-l
                _overlay = new PerformanceOverlayView();
            }

            // Elimină toate instanțele existente de PerformanceOverlayView
            foreach (var child in layout.Children.OfType<PerformanceOverlayView>().ToList())
            {
                layout.Children.Remove(child);
            }

            //Adauga overlay
            layout.Children.Add(_overlay);
            _overlay.Start();


            //if (!_overlay!.IsVisibleInLayout(layout))
            //{
            //    // Scoate dacă există deja și adaugă la final → on top
            //    if (layout.Children.Contains(_overlay))
            //        layout.Children.Remove(_overlay);

            //    layout.Children.Add(_overlay);
            //    _overlay.Start();
            //}
        }
    }
}



