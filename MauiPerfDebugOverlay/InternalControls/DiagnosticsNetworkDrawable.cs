using MauiPerfDebugOverlay.Extensions;
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
        private readonly Dictionary<int, RectF> _aiButtonRects = new();
        private readonly Dictionary<int, RectF> _checkboxRects = new();
        private readonly HashSet<int> _selectedMetrics = new();
        private readonly HashSet<int> _aiClickedFeedback = new();

        private RectF? _globalCheckboxRect;
        private RectF? _globalAskAIButtonRect;
        private bool _selectAllState = false;
        private bool _globalAskClickedFeedback = false;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FontSize = 14;
            canvas.Font = Microsoft.Maui.Graphics.Font.Default;

            _rects.Clear();
            _aiButtonRects.Clear();
            _checkboxRects.Clear();
            _globalCheckboxRect = null;
            _globalAskAIButtonRect = null;

            float y = StartY;
            var items = DiagnosticsListener.Instance.GetAllNetwork();

            if (items.Count == 0)
            {
                string line = "No metrics collected on Network from System.Diagnostics.Metrics yet.";
                var rect = new RectF(StartX, y - LineHeight / 2, dirtyRect.Width - 20, LineHeight);
                canvas.FontColor = Colors.White;
                canvas.DrawString(line, rect, HorizontalAlignment.Left, VerticalAlignment.Top);
                return;
            }

            y = DrawMetricsSection(canvas, "Network metrics", items, y, dirtyRect);
        }

        private float DrawMetricsSection(ICanvas canvas, string title, IReadOnlyList<NetworkMetric> items, float y, RectF dirtyRect)
        {
            if (items.Count == 0) return y;

            // Header rect
            var rect = new RectF(2, y - LineHeight / 2, dirtyRect.Width - 4, LineHeight);
            canvas.FillColor = Color.FromHex("D98880");
            canvas.FillRectangle(rect);

            // Checkbox global (select/deselect all)
            if (PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ViewTabAI)
            {
                float checkboxSize = LineHeight / 2;
                var checkboxRect = new RectF(StartX, y - LineHeight / 2 + 10, checkboxSize, checkboxSize);
                canvas.StrokeColor = Colors.DarkGray;
                canvas.StrokeSize = 1;
                canvas.DrawRoundedRectangle(checkboxRect, 6);

                if (_selectAllState)
                {
                    canvas.FontColor = Colors.Lime;
                    canvas.FontSize = 18;
                    canvas.DrawString("✓", new RectF(checkboxRect.X, checkboxRect.Y - 2, checkboxRect.Width, checkboxRect.Height),
                                      HorizontalAlignment.Center, VerticalAlignment.Center);
                }

                _globalCheckboxRect = checkboxRect;

                // Label pentru checkbox
                canvas.FontColor = Colors.White;
                canvas.FontSize = 14;
                canvas.DrawString("Select/Deselect All",
                                  new RectF(checkboxRect.Right + 5, y - LineHeight / 2, 160, LineHeight),
                                  HorizontalAlignment.Left, VerticalAlignment.Center);
            }

            // Titlu (pus după checkbox ca să fie mereu la stânga lui, vizibil și clar)
            canvas.FontColor = Colors.White;
            canvas.FontSize = 16;
            canvas.DrawString(title, rect, HorizontalAlignment.Center, VerticalAlignment.Center);

            // [Ask AI] global (pus la final în dreapta sus)
            if (PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ViewTabAI)
            {
                string aiButton = "[Ask AI]";
                float aiButtonWidth = 70;
                _globalAskAIButtonRect = new RectF(dirtyRect.Width - aiButtonWidth - 5, y - LineHeight / 2, aiButtonWidth, LineHeight);

                canvas.FontColor = _globalAskClickedFeedback ? Colors.Gray : Colors.Cyan;
                canvas.DrawString(aiButton, _globalAskAIButtonRect.Value, HorizontalAlignment.Left, VerticalAlignment.Center);
            }

            y += LineHeight;

            // Liniile cu metrice
            foreach (var kvp in items)
            {
                _rects[kvp.Id] = new RectF(0, y - LineHeight / 2, dirtyRect.Width, LineHeight);
                canvas.FillColor = _selectedMetrics.Contains(kvp.Id)
                    ? Colors.DarkSlateGray
                    : Color.FromArgb("#BB222222");
                canvas.FillRectangle(_rects[kvp.Id]);

                // Checkbox per rând
                if (PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ViewTabAI)
                {
                    float checkboxSize = LineHeight / 2;
                    var checkboxRect = new RectF(StartX, y - LineHeight / 2 + 10, checkboxSize, checkboxSize);
                    canvas.StrokeColor = Colors.DarkGray;
                    canvas.StrokeSize = 1;
                    canvas.DrawRoundedRectangle(checkboxRect, 6);

                    if (_selectedMetrics.Contains(kvp.Id))
                    {
                        canvas.FontColor = Colors.Lime;
                        canvas.FontSize = 18;
                        canvas.DrawString("✓", new RectF(checkboxRect.X, checkboxRect.Y - 2, checkboxRect.Width, checkboxRect.Height),
                                          HorizontalAlignment.Center, VerticalAlignment.Center);
                    }

                    _checkboxRects[kvp.Id] = checkboxRect;
                }

                // Expand/collapse symbol
                string expandSymbol = (kvp.Tags != null && kvp.Tags.Length > 0)
                    ? (kvp.IsExpanded ? "[-]" : "[+]") : "   ";
                var symbolRect = new RectF(StartX + 30, y - LineHeight / 2, 30, LineHeight);
                canvas.FontColor = Colors.Orange;
                canvas.FontSize = 14;
                canvas.DrawString(expandSymbol, symbolRect, HorizontalAlignment.Left, VerticalAlignment.Center);

                // Text metrică
                string line = $"[{kvp.Timestamp:HH:mm:ss.fff}] {kvp.Name.Replace("dotnet.", "")} = {kvp.Value}";
                var textRect = new RectF(StartX + 60, y - LineHeight / 2, dirtyRect.Width - 130, LineHeight);
                canvas.FontColor = Colors.White;
                canvas.FontSize = 14;
                canvas.DrawString(line, textRect, HorizontalAlignment.Left, VerticalAlignment.Center);

                // [Ask AI] pe rând
                if (PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ViewTabAI)
                {
                    string aiButton = "[Ask AI]";
                    float aiButtonX = dirtyRect.Width - 70;
                    canvas.FontColor = _aiClickedFeedback.Contains(kvp.Id) ? Colors.Gray : Colors.Cyan;
                    var aiRect = new RectF(aiButtonX, y - LineHeight / 2, 60, LineHeight);
                    canvas.DrawString(aiButton, aiRect, HorizontalAlignment.Left, VerticalAlignment.Center);
                    _aiButtonRects[kvp.Id] = aiRect;
                }

                y += LineHeight;

                // Tag-uri expandate
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

        // Hit tests
        public bool HitTestCheckbox(int id, float x, float y) =>
            PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ViewTabAI
            && _checkboxRects.TryGetValue(id, out var rect)
            && rect.Contains(x, y);

        public List<int> HitTestAI(float x, float y)
        {
            if (!PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ViewTabAI)
                return new List<int>();

            foreach (var kvp in _aiButtonRects)
                if (kvp.Value.Contains(x, y))
                    return new List<int> { kvp.Key };
            return new List<int>();
        }

        public int HitTest(float x, float y)
        {
            foreach (var kvp in _rects)
                if (kvp.Value.Contains(x, y))
                    return kvp.Key;
            return 0;
        }

        public bool HitTestGlobalCheckbox(float x, float y) =>
            _globalCheckboxRect.HasValue && _globalCheckboxRect.Value.Contains(x, y);

        public bool HitTestGlobalAI(float x, float y) =>
            _globalAskAIButtonRect.HasValue && _globalAskAIButtonRect.Value.Contains(x, y);

        // Selecții
        public void SelectMetric(int id) => _selectedMetrics.Add(id);
        public void DeselectMetric(int id) => _selectedMetrics.Remove(id);
        public bool IsMetricSelected(int id) => _selectedMetrics.Contains(id);
        public void ClearSelection() => _selectedMetrics.Clear();

        public void ToggleSelectAll()
        {
            _selectAllState = !_selectAllState;
            if (_selectAllState)
            {
                foreach (var id in _rects.Keys)
                    _selectedMetrics.Add(id);
            }
            else
            {
                _selectedMetrics.Clear();
            }
        }

        // Feedback vizual
        public void MarkAIClicked(int id, GraphicsView graphicsView)
        {
            _aiClickedFeedback.Add(id);
            graphicsView.Invalidate();

            Task.Delay(300).ContinueWith(_ =>
            {
                _aiClickedFeedback.Remove(id);
                graphicsView.Invalidate();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

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

        // Utilitare
        public List<NetworkMetric> GetSelectedMetrics()
        {
            var items = DiagnosticsListener.Instance.GetAllNetwork();
            return items.Where(m => _selectedMetrics.Contains(m.Id)).ToList();
        }

        public IEnumerable<int> GetAllMetricIds() => _rects.Keys;

        internal double NewHeight() => DiagnosticsListener.Instance.NewHeight();
    }
}
