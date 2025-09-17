using MauiPerfDebugOverlay.Models.Internal;
using MauiPerfDebugOverlay.Services;

namespace MauiPerfDebugOverlay.InternalControls
{
    public class DiagnosticsNetworkDrawable : IDrawable
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

            var items = DiagnosticsListener.Instance.GetAllNetwork(); 

            if (items.Count > 0 )
            {
                y = DrawMetricsSection(canvas, "Network metrics", items, y, dirtyRect); 
            }
            else
            {
                string line = "No metrics collected from System.Diagnostics.Metrics yet on Network.";
                var rect = new RectF(StartX, y - LineHeight / 2, dirtyRect.Width - 20, LineHeight);
                canvas.FontColor = Colors.White;
                canvas.DrawString(line, rect, HorizontalAlignment.Left, VerticalAlignment.Top);
            }
        }

        private float DrawMetricsSection(ICanvas canvas, string title, IReadOnlyList<NetworkMetric> items, float y, RectF dirtyRect)
        {
            if (items.Count == 0)
                return y;

            // titlu
            var rect = new RectF(2, y - LineHeight / 2, dirtyRect.Width - 4, LineHeight);
            canvas.StrokeColor = Color.FromHex("D98880");
            canvas.FillColor = Color.FromHex("D98880");
            canvas.FillRectangle(rect);

            canvas.FontColor = Colors.White;
            canvas.FontSize = 16;
            canvas.DrawString(title, rect, HorizontalAlignment.Center, VerticalAlignment.Center);
            y += LineHeight;

            int index = 0;
            foreach (var kvp in items)
            {
                index++;
                string line = $"{kvp.Name.Replace("dotnet.", "")} = {kvp.Value}";

                rect = new RectF(StartX, y - LineHeight / 2, dirtyRect.Width - 20, LineHeight);
                _rects[kvp.Name] = rect;

                // fundal alternativ
                if (index % 2 == 0)
                {
                    canvas.FillColor = Color.FromArgb("#BB222222");
                    canvas.FillRectangle(new RectF(0, y - LineHeight / 2, dirtyRect.Width, LineHeight));
                }

                canvas.FontColor = Colors.White;
                canvas.FontSize = 14;
                canvas.DrawString(line, rect, HorizontalAlignment.Left, VerticalAlignment.Center);
                y += LineHeight;
            }

            return y;
        }
         
        public int CountMetrics() => DiagnosticsListener.Instance.CountNetwork();
    }
}
