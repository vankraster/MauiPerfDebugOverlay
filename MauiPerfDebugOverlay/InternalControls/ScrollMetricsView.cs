using MauiPerfDebugOverlay.Services;

namespace MauiPerfDebugOverlay.InternalControls
{
    public class ScrollMetricsView : ContentView
    {
        internal readonly GraphicsView _graphicsView;

        public ScrollMetricsView()
        {
            _graphicsView = new GraphicsView
            {
                HeightRequest = 200,
                WidthRequest = 800,
                VerticalOptions = LayoutOptions.Start
            };

            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Both,
                Content = _graphicsView
            };
            var metricsDrawable = new ScrollMetricsDrawable();
            _graphicsView.Drawable = metricsDrawable;

            ScrollMetricsBuffer.Instance.CollectionChanged += Instance_CollectionChanged;

        }

        private void Instance_CollectionChanged(string arg1, Guid? arg2, double? arg3, double? arg4, bool? arg5)
        {
            if (this.IsVisible)
                Refresh();
        }

        /// <summary>
        /// Reîmprospătează datele și redesenează controlul
        /// </summary>
        public void Refresh()
        { 
            var metricsDrawable = (_graphicsView.Drawable as ScrollMetricsDrawable);
            // Înălțimea ajustată dinamic
            _graphicsView.HeightRequest = (metricsDrawable.CountMetrics() + 1) * ScrollMetricsDrawable.LineHeight;
            _graphicsView.Invalidate();
        }
    }
}
