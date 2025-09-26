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
                WidthRequest = 800,
                VerticalOptions = LayoutOptions.Start
            };

            var metricsDrawable = new DiagnosticsMetricsDrawable();
            _graphicsView.Drawable = metricsDrawable;

            _graphicsView.StartInteraction += (s, e) =>
            {
                Tapped(e.Touches[0].X, e.Touches[0].Y);
            };

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

        private void Tapped(float x, float y)
        {
            if (_graphicsView.Drawable is not DiagnosticsMetricsDrawable drawable)
                return;

            // Hit-test pe header
            var header = drawable.HitTestHeaderAI(x, y);
            if (header != null)
            {
                var metricsForHeader =
                    header == "Exception metrics"
                        ? DiagnosticsListener.Instance.GetAllExceptions()
                        : DiagnosticsListener.Instance.GetAll();

                GeminiService.Instance.AskForMetrics(metricsForHeader);
                return;
            }

            // Hit-test pe linie
            var lineKey = drawable.HitTestLineAI(x, y);
            if (lineKey != null)
            {
                var items = DiagnosticsListener.Instance.GetAll();
                var exceptions = DiagnosticsListener.Instance.GetAllExceptions();

                if (items.ContainsKey(lineKey))
                    GeminiService.Instance.AskForSingleMetric(lineKey, items[lineKey]);
                else if (exceptions.ContainsKey(lineKey))
                    GeminiService.Instance.AskForSingleMetric(lineKey, exceptions[lineKey]);

                drawable.MarkLineAIClicked(lineKey, _graphicsView); // feedback vizual
                return;
            }
        }

        /// <summary>
        /// Reîmprospătează datele și redesenează controlul
        /// </summary>
        public void Refresh()
        {
            if (_graphicsView.Drawable is DiagnosticsMetricsDrawable metricsDrawable)
            {
                var newHeight = ((metricsDrawable?.CountMetrics() ?? 0) + 3) * DiagnosticsMetricsDrawable.LineHeight;
                if (newHeight != _graphicsView.HeightRequest)
                    _graphicsView.HeightRequest = newHeight;

                _graphicsView.Invalidate();
            }
        }
    }
}
