namespace MauiPerfDebugOverlay.InternalControls
{
    public class AIViewControl : ContentView
    {
        internal readonly GraphicsView _graphicsView;
        internal readonly Label _label;
        //internal readonly AIDrawable _aiDrawable;
        public AIViewControl()
        {
            _graphicsView = new GraphicsView
            {
                HeightRequest = 180,
                WidthRequest = 800,
                VerticalOptions = LayoutOptions.Start,
            };

            _graphicsView.StartInteraction += (s, e) =>
            {
                Tapped(e.Touches[0].X, e.Touches[0].Y);
            };
            //_aiDrawable = new AIDrawable();
            //_graphicsView.Drawable = _aiDrawable;

            _label = new Label
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
            };

            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Both,
                Content = _label // _graphicsView
            };
        }


        public void Tapped(float x, float y)
        {
            if (_graphicsView.Drawable is TreeDrawable drawable)
            {
                var clickedNode = drawable.HitTest(x, y);
                if (clickedNode != null)
                {

                    _graphicsView.Invalidate();
                }
            }
        }
    }
}
