using MauiPerfDebugOverlay.Extensions;
using MauiPerfDebugOverlay.Services;

namespace MauiPerfDebugOverlay.InternalControls
{
    public class DiagnosticsNetworkView : ContentView
    {
        internal readonly GraphicsView _graphicsView;

        public DiagnosticsNetworkView()
        {
            _graphicsView = new GraphicsView
            {
                HeightRequest = 200,
                WidthRequest = PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ViewTabAI ? 600 : 520,
                VerticalOptions = LayoutOptions.Start
            };

            _graphicsView.Drawable = new DiagnosticsNetworkDrawable();

            _graphicsView.StartInteraction += (s, e) =>
            {
                Tapped(e.Touches[0].X, e.Touches[0].Y);
            };

            DiagnosticsListener.Instance.CollectionNetworkChanged += Instance_CollectionChanged;

            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Both,
                Content = _graphicsView
            };
        }

        public void Tapped(float x, float y)
        {
            if (_graphicsView.Drawable is DiagnosticsNetworkDrawable drawable)
            {
                // [Ask AI] global
                if (drawable.HitTestGlobalAI(x, y))
                {
                    drawable.MarkGlobalAIClicked(_graphicsView);

                    var selectedData = drawable.GetSelectedMetrics();
                    if (!selectedData.Any())
                        selectedData = DiagnosticsListener.Instance.GetAllNetwork().ToList();

                    if (selectedData.Any())
                        GeminiService.Instance.AskForNetworkMetrics(selectedData);

                    drawable.ClearSelection();
                    _graphicsView.Invalidate();
                    return;
                }

                // Checkbox global
                if (drawable.HitTestGlobalCheckbox(x, y))
                {
                    drawable.ToggleSelectAll();
                    _graphicsView.Invalidate();
                    return;
                }

                // [Ask AI] per metric
                var clickedMetrics = drawable.HitTestAI(x, y);
                if (clickedMetrics.Any())
                {
                    foreach (var id in clickedMetrics)
                    {
                        if (!drawable.IsMetricSelected(id))
                            drawable.SelectMetric(id);

                        drawable.MarkAIClicked(id, _graphicsView);
                    }

                    _graphicsView.Invalidate();

                    var selectedData = drawable.GetSelectedMetrics();
                    if (selectedData.Any())
                        GeminiService.Instance.AskForNetworkMetrics(selectedData);

                    drawable.ClearSelection();
                    _graphicsView.Invalidate();
                    return;
                }

                // Checkbox per metric
                foreach (var id in drawable.GetAllMetricIds())
                {
                    if (drawable.HitTestCheckbox(id, x, y))
                    {
                        if (drawable.IsMetricSelected(id))
                            drawable.DeselectMetric(id);
                        else
                            drawable.SelectMetric(id);

                        _graphicsView.Invalidate();
                        return;
                    }
                }

                // Expand/collapse linie
                var clickedNode = drawable.HitTest(x, y);
                if (clickedNode > 0)
                {
                    DiagnosticsListener.Instance.CollapseExpandNetwork(clickedNode);
                    Refresh();
                }
            }
        }

        private void Instance_CollectionChanged(string arg1, string? arg2, object? arg3)
        {
            if (this.IsVisible)
                Application.Current.Dispatcher.Dispatch(Refresh);
        }

        public void Refresh()
        {
            if (_graphicsView.Drawable is DiagnosticsNetworkDrawable metricsDrawable)
            {
                var newHeight = metricsDrawable.NewHeight();
                if (newHeight != _graphicsView.HeightRequest)
                    _graphicsView.HeightRequest = newHeight;

                _graphicsView.Invalidate();
            }
        }
    }
}
