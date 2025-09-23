using MauiPerfDebugOverlay.Extensions;

namespace MauiPerfDebugOverlay.InternalControls
{
    internal class AIDrawable : IDrawable
    {
        internal const float LineHeight = 36;
        private const float StartX = 10;
        private const float StartY = 20;
        private readonly Dictionary<string, RectF> _rects = new();



        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FontSize = 14;
            canvas.Font = Microsoft.Maui.Graphics.Font.Default;

            _rects.Clear();
            float y = StartY;

            if (!PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ViewTabAI)
            {
                var lines = new string[] {
                              "In order to have AI analyzer you should add Gemini ApiKey in ",
                              "MauiProgram > .UsePerformanceDebugOverlay property GeminiAPIKey ",
                              "You can generate key in https://aistudio.google.com/apikey "};

                var rect = new RectF(StartX, y - LineHeight / 2, dirtyRect.Width - 20, LineHeight);
                canvas.FontColor = Colors.White;
                //canvas.DrawString(line, rect, HorizontalAlignment.Left, VerticalAlignment.Top);
            }
            else
            {

            }
        }

    }
}