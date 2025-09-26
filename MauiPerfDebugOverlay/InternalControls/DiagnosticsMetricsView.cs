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

        public void Refresh()
        {
            var metricsDrawable = (_graphicsView.Drawable as DiagnosticsMetricsDrawable);
            var newHeight = ((metricsDrawable?.CountMetrics() ?? 0) + 1) * DiagnosticsMetricsDrawable.LineHeight;
            if (newHeight != _graphicsView.HeightRequest)
                _graphicsView.HeightRequest = newHeight;

            _graphicsView.Invalidate();
        }

        private void Tapped(float x, float y)
        {
            if (_graphicsView.Drawable is DiagnosticsMetricsDrawable drawable)
            {
                // Click global [Ask AI]
                if (drawable.HitTestGlobalAI(x, y))
                {
                    drawable.MarkGlobalAIClicked(_graphicsView);

                    var allMetrics = DiagnosticsListener.Instance.GetAll()
                        .Concat(DiagnosticsListener.Instance.GetAllExceptions())
                        .ToDictionary(k => k.Key, v => v.Value);

                    GeminiService.Instance.AskForMetrics(allMetrics);
                    return;
                }

                // Click pe rând [Ask AI]
                var rowId = drawable.HitTestRowAI(x, y);
                if (rowId != null)
                {
                    drawable.MarkRowAIClicked(rowId, _graphicsView);

                    var metric = DiagnosticsListener.Instance.GetAll()
                        .Concat(DiagnosticsListener.Instance.GetAllExceptions())
                        .FirstOrDefault(m => m.Key == rowId);

                    if (!metric.Equals(default(KeyValuePair<string, object>)))
                        GeminiService.Instance.AskForSingleMetric(metric.Key, metric.Value);

                    return;
                }
            }
        }
    }
}
