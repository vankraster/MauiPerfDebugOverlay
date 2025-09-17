using MauiPerfDebugOverlay.Models.Internal;
using MauiPerfDebugOverlay.Services;

namespace MauiPerfDebugOverlay.InternalControls
{
    public class DiagnosticsNetworkDrawable : IDrawable
    {
        internal const float LineHeight = 36;
        private const float StartX = 10;
        private const float StartY = 20;
        private readonly Dictionary<int, RectF> _rects = new();

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FontSize = 14;
            canvas.Font = Microsoft.Maui.Graphics.Font.Default;

            _rects.Clear();
            float y = StartY;

            var items = DiagnosticsListener.Instance.GetAllNetwork();

            if (items.Count > 0)
            {
                y = DrawMetricsSection(canvas, "Network metrics", items, y, dirtyRect);
            }
            else
            {
                string line = "No metrics collected on Network from System.Diagnostics.Metrics yet.";
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
                string line = $"[{kvp.Timestamp:HH:mm:ss.fff}] {kvp.Name.Replace("dotnet.", "")} = {kvp.Value}";


                // rect pentru întreaga linie a metricii
                //rect = new RectF(StartX, y - LineHeight / 2, dirtyRect.Width - 20, LineHeight);


                _rects[kvp.Id] = new RectF(0, y - LineHeight / 2, dirtyRect.Width, LineHeight);
                canvas.FillColor = Color.FromArgb("#BB222222");
                canvas.FillRectangle(_rects[kvp.Id]);


                // desenăm simbol expand/collapse
                string expandSymbol = (kvp.Tags != null && kvp.Tags.Length > 0)
                    ? (kvp.IsExpanded ? "[-]" : "[+]")
                    : "   "; // gol dacă nu are tags

                var symbolRect = new RectF(StartX, y - LineHeight / 2, 30, LineHeight);
                canvas.FontColor = Colors.Orange;
                canvas.FontSize = 14;
                canvas.DrawString(expandSymbol, symbolRect, HorizontalAlignment.Left, VerticalAlignment.Center);

                // desenăm textul metricii
                var textRect = new RectF(StartX + 35, y - LineHeight / 2, dirtyRect.Width - 55, LineHeight);
                canvas.FontColor = Colors.White;
                canvas.FontSize = 14;
                canvas.DrawString(line, textRect, HorizontalAlignment.Left, VerticalAlignment.Center);

                y += LineHeight;

                // dacă e expandat => afișăm tag-urile indentate
                if (kvp.IsExpanded && kvp.Tags != null && kvp.Tags.Length > 0)
                {
                    foreach (var tag in kvp.Tags)
                    {
                        string tagLine = $"• {tag.Key} = {tag.Value}";
                        var tagRect = new RectF(StartX + 50, y - LineHeight / 2, dirtyRect.Width - 70, LineHeight);

                        canvas.FontColor = Colors.LightGray;
                        canvas.FontSize = 12;
                        canvas.DrawString(tagLine, tagRect, HorizontalAlignment.Left, VerticalAlignment.Center);

                        y += LineHeight * 0.8f;
                    }
                }
            }

            return y;
        }

        public int HitTest(float x, float y)
        {
            foreach (var kvp in _rects )
            {
                if (kvp.Value.Contains(x, y))
                    return kvp.Key;
            }
            return 0;
        }
         

        internal double NewHeight() => DiagnosticsListener.Instance.NewHeight();
    }
}
