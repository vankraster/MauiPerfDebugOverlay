using MauiPerfDebugOverlay.Services;

namespace MauiPerfDebugOverlay.InternalControls
{
    public class DiagnosticsMetricsDrawable : IDrawable
    {
        internal const float LineHeight = 36;
        private const float StartX = 10;
        private const float StartY = 20;

        private readonly Dictionary<string, RectF> _rects = new();
        private readonly Dictionary<string, RectF> _aiButtonRects = new();   // [Ask AI] pe rânduri
        private RectF? _globalAskAIButtonRect = null;                        // [Ask AI] global
        private readonly HashSet<string> _aiClickedFeedback = new();         // feedback pe rând
        private bool _globalAskClickedFeedback = false;                      // feedback global

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FontSize = 14;
            canvas.Font = Microsoft.Maui.Graphics.Font.Default;

            _rects.Clear();
            _aiButtonRects.Clear();
            _globalAskAIButtonRect = null;

            float y = StartY;

            var items = DiagnosticsListener.Instance.GetAll();
            var itemsExceptions = DiagnosticsListener.Instance.GetAllExceptions();

            if (items.Count > 0 || itemsExceptions.Count > 0)
            {
                y = DrawMetricsSection(canvas, "Exception metrics", itemsExceptions, y, dirtyRect);
                y = DrawMetricsSection(canvas, "Generic metrics", items, y, dirtyRect);
            }
            else
            {
                string line = "No metrics collected from System.Diagnostics.Metrics yet.";
                var rect = new RectF(StartX, y - LineHeight / 2, dirtyRect.Width - 20, LineHeight);
                canvas.FontColor = Colors.White;
                canvas.DrawString(line, rect, HorizontalAlignment.Left, VerticalAlignment.Top);
            }
        }

        private float DrawMetricsSection(ICanvas canvas, string title, IReadOnlyDictionary<string, object> items, float y, RectF dirtyRect)
        {
            if (items.Count == 0)
                return y;

            // Header rect
            var rect = new RectF(2, y - LineHeight / 2, dirtyRect.Width - 4, LineHeight);
            canvas.FillColor = Color.FromHex("D98880");
            canvas.FillRectangle(rect);

            // Titlu
            canvas.FontColor = Colors.White;
            canvas.FontSize = 16;
            canvas.DrawString(title, rect, HorizontalAlignment.Center, VerticalAlignment.Center);

            // [Ask AI] global în dreapta
            string aiButton = "[Ask AI]";
            float aiButtonWidth = 70;
            _globalAskAIButtonRect = new RectF(dirtyRect.Width - aiButtonWidth - 5, y - LineHeight / 2, aiButtonWidth, LineHeight);
            canvas.FontColor = _globalAskClickedFeedback ? Colors.Gray : Colors.Cyan;
            canvas.DrawString(aiButton, _globalAskAIButtonRect.Value, HorizontalAlignment.Left, VerticalAlignment.Center);

            y += LineHeight;

            int index = 0;
            foreach (var kvp in items)
            {
                index++;
                string line = $"{kvp.Key.Replace("dotnet.", "")} = {kvp.Value}";

                rect = new RectF(StartX, y - LineHeight / 2, dirtyRect.Width - 100, LineHeight);
                _rects[kvp.Key] = rect;

                // fundal alternativ
                if (index % 2 == 0)
                {
                    canvas.FillColor = Color.FromArgb("#BB222222");
                    canvas.FillRectangle(new RectF(0, y - LineHeight / 2, dirtyRect.Width, LineHeight));
                }

                // textul metricii
                canvas.FontColor = Colors.White;
                canvas.FontSize = 14;
                canvas.DrawString(line, rect, HorizontalAlignment.Left, VerticalAlignment.Center);

                // [Ask AI] pentru rând
                string rowAiButton = "[Ask AI]";
                var aiRect = new RectF(dirtyRect.Width - 70, y - LineHeight / 2, 60, LineHeight);
                canvas.FontColor = _aiClickedFeedback.Contains(kvp.Key) ? Colors.Gray : Colors.Cyan;
                canvas.DrawString(rowAiButton, aiRect, HorizontalAlignment.Left, VerticalAlignment.Center);
                _aiButtonRects[kvp.Key] = aiRect;

                y += LineHeight;
            }

            return y;
        }

        // Hit-test pentru [Ask AI] global
        public bool HitTestGlobalAI(float x, float y) =>
            _globalAskAIButtonRect.HasValue && _globalAskAIButtonRect.Value.Contains(x, y);

        // Hit-test pentru [Ask AI] pe rânduri
        public string? HitTestRowAI(float x, float y)
        {
            foreach (var kvp in _aiButtonRects)
                if (kvp.Value.Contains(x, y))
                    return kvp.Key;
            return null;
        }

        // Feedback pentru click global
        public void MarkGlobalAIClicked(GraphicsView graphicsView)
        {
            _globalAskClickedFeedback = true;
            graphicsView.Invalidate();

            Task.Delay(300).ContinueWith(_ =>
            {
                _globalAskClickedFeedback = false;
                graphicsView.Invalidate();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        // Feedback pentru click pe rând
        public void MarkRowAIClicked(string id, GraphicsView graphicsView)
        {
            _aiClickedFeedback.Add(id);
            graphicsView.Invalidate();

            Task.Delay(300).ContinueWith(_ =>
            {
                _aiClickedFeedback.Remove(id);
                graphicsView.Invalidate();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public int CountMetrics() => DiagnosticsListener.Instance.Count();
    }
}
