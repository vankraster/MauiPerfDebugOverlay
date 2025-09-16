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
                Application.Current.Dispatcher.Dispatch(Refresh);
        }



        /// <summary>
        /// Reîmprospătează datele și redesenează controlul
        /// </summary>
        public void Refresh()
        {
            var metricsDrawable = (_graphicsView.Drawable as DiagnosticsMetricsDrawable);
            // Înălțimea ajustată dinamic
            var newHeight = ((metricsDrawable?.CountMetrics() ?? 0) + 1) * DiagnosticsMetricsDrawable.LineHeight;
            if (newHeight != _graphicsView.HeightRequest)
                _graphicsView.HeightRequest = newHeight;

            var newWidth = metricsDrawable.HasHttp() ? 700 : 520;
            if (newWidth != _graphicsView.WidthRequest)
                _graphicsView.WidthRequest = newWidth;

            _graphicsView.Invalidate();
        }
    }
}
