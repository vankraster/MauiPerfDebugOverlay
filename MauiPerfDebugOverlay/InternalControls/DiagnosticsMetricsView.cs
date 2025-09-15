using MauiPerfDebugOverlay.Services;

namespace MauiPerfDebugOverlay.InternalControls
{
    public class DiagnosticsMetricsView : ContentView
    {
        internal readonly GraphicsView _graphicsView;

        public DiagnosticsMetricsView()
        {
            _graphicsView = new GraphicsView
            {
                HeightRequest = 200,
                WidthRequest = 520,
                VerticalOptions = LayoutOptions.Start
            };
            var metricsDrawable = new DiagnosticsMetricsDrawable();
            _graphicsView.Drawable = metricsDrawable;

            DiagnosticsListener.Instance.CollectionChanged += Instance_CollectionChanged; 

            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Both,
                Content = _graphicsView
            };
        }

        private void Instance_CollectionChanged(string arg1, string? arg2, object? arg3)
        {
            if (this.IsVisible)
                Refresh();
        }



        /// <summary>
        /// Reîmprospătează datele și redesenează controlul
        /// </summary>
        public void Refresh()
        {
            var metricsDrawable = (_graphicsView.Drawable as DiagnosticsMetricsDrawable);
            // Înălțimea ajustată dinamic
            _graphicsView.HeightRequest = (metricsDrawable.CountMetrics() + 1) * DiagnosticsMetricsDrawable.LineHeight;
            _graphicsView.Invalidate();
        }
    }
}
