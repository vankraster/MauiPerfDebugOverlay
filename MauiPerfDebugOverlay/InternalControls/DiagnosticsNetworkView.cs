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
                WidthRequest = 520,
                VerticalOptions = LayoutOptions.Start
            };
            var metricsDrawable = new DiagnosticsNetworkDrawable();
            _graphicsView.Drawable = metricsDrawable;

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

        /// <summary>
        /// Reîmprospătează datele și redesenează controlul
        /// </summary>
        public void Refresh()
        {
            var metricsDrawable = (_graphicsView.Drawable as DiagnosticsNetworkDrawable);
            // Înălțimea ajustată dinamic
            var newHeight = metricsDrawable.NewHeight();
            if (newHeight != _graphicsView.HeightRequest)
                _graphicsView.HeightRequest = newHeight;

            _graphicsView.Invalidate();
        }
    }
}
