using MauiPerfDebugOverlay.Services;

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
                HorizontalOptions = LayoutOptions.StartAndExpand,
                VerticalOptions = LayoutOptions.StartAndExpand,
                Text = "⚡ To enable AI Analyzer, please add your Gemini API Key.\n" +
                        "\n" +
                        "1. Open MauiProgram.cs and set it in:\n" +
                        "   .UsePerformanceDebugOverlay(new Models.PerformanceOverlayOptions { GeminiAPIKey: \"your_key_here\" .....)\n" +
                        "\n" +
                        "2. You can generate a free API key here:\n" +
                        "   https://aistudio.google.com/apikey\n" +
                        "\n" +
                        "💡 Once set, the [Ask AI] button will become visible in the Tree Tab."
            };

            GeminiService.Instance.ResponseChanged += Instance_ResponseChanged;
            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Both,
                Content = _label // _graphicsView
            };
        }

        private void Instance_ResponseChanged(string obj)
        {
            _label.Text = obj;
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
