using MauiPerfDebugOverlay.Extensions;
using MauiPerfDebugOverlay.Models.Internal;
using MauiPerfDebugOverlay.Services;
using MauiPerfDebugOverlay.Utils;

namespace MauiPerfDebugOverlay.InternalControls
{
    public class TreeDrawable : IDrawable
    {
        private readonly TreeNode _root;
        private readonly GraphicsView _graphics;
        internal const float LineHeight = 26;
        private const float StartX = 10;

        // pentru hit testing
        private readonly Dictionary<TreeNode, RectF> _nodeRects = new();
        private readonly Dictionary<TreeNode, RectF> _aiButtonRects = new();

        public TreeDrawable(GraphicsView graphics, TreeNode root)
        {
            _root = root;
            _graphics = graphics;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FontSize = 14;
            canvas.FontColor = Colors.White;
            canvas.Font = Microsoft.Maui.Graphics.Font.Default;

            _nodeRects.Clear();
            float y = 20;

            DrawAsciiTree(canvas, _root, 0, ref y);
        }


        private void DrawAsciiTree(ICanvas canvas, TreeNode node, int level, ref float y)
        {
            const float indentPerLevel = 22f;
            float startXNode = StartX + level * indentPerLevel;


            var selfMs = LoadTimeMetricsStore.Instance.GetSelfMsByNode(node);

            // 1️⃣ simbol expand/collapse
            string expandSymbol = node.Children.Count > 0 ? (node.IsExpanded ? "[-]" : "[+]") : "   ";
            canvas.FontColor = node.Children.Count > 0 ? Colors.Orange : Colors.White;
            canvas.DrawString(expandSymbol, new RectF(startXNode, y, 30, LineHeight),
                              HorizontalAlignment.Left, VerticalAlignment.Top);

            // 2️⃣ text nod
            float textX = startXNode + 35;
            canvas.FontColor = Colors.White;
            canvas.DrawString(node.Name, new RectF(textX, y, 200, LineHeight),
                              HorizontalAlignment.Left, VerticalAlignment.Top);

            // 3️⃣ indicator de prag
            string indicator = "";
            Color indicatorColor = Colors.White;

            if (selfMs > PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.LoadTimeDanger)
            {
                indicator = "⛔";
                indicatorColor = Color.FromHex("D98880");
            }
            else if (selfMs > PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.LoadTimeWarning)
            {
                indicator = "⚠";
                indicatorColor = Color.FromHex("FFECB3");
            }

            string timeText = $"{selfMs.FormatTime()}";
            canvas.FontColor = Colors.LightGray;
            var timeRect = new RectF(textX + 180, y, 80, LineHeight);
            canvas.DrawString(timeText, timeRect,
                              HorizontalAlignment.Left, VerticalAlignment.Top);

            var timeWidth = canvas.GetStringSize(timeText, Microsoft.Maui.Graphics.Font.Default, 14).Width;
            var lastX = timeRect.X + timeWidth;
            // [AI] vine după textul timpului
            if (PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ViewTabAI)
            {
                string aiButton = "[Ask AI]";
                lastX += 10; // spațiu de 10px după timp
                canvas.FontColor = _aiButtonClicked.Contains(node) ? Colors.Gray : Colors.Cyan;
                canvas.DrawString(aiButton, new RectF(lastX, y, 55, LineHeight),
                                  HorizontalAlignment.Left, VerticalAlignment.Top);

                _aiButtonRects[node] = new RectF(lastX, y, 40, LineHeight);
                lastX += 45;
            }

            canvas.FontColor = indicatorColor;
            canvas.DrawString(indicator, new RectF(lastX + 10, y, 20, LineHeight),
                              HorizontalAlignment.Left, VerticalAlignment.Top);
            lastX += 30;


            // 5️⃣ bară de progres

            float maxBarWidth = 150;
            float barWidth = Math.Max(Math.Min(maxBarWidth, (float)(selfMs / 1000 * maxBarWidth)), 1);
            Color barColor = selfMs > PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.LoadTimeDanger
                ? Colors.Red
                : selfMs > PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.LoadTimeWarning
                    ? Colors.Yellow
                    : Colors.Green;

            canvas.FillColor = barColor;
            canvas.FillRectangle(new RectF(lastX + 10, y + 4, barWidth, LineHeight - 8));


            // 6️⃣ salvăm rect pentru hit-testing
            var rect = new RectF(startXNode, y, 500, LineHeight);
            _nodeRects[node] = rect;

            y += LineHeight;

            // 7️⃣ desenăm copiii dacă nodul este expandat
            if (node.IsExpanded)
            {
                foreach (var child in node.Children)
                {
                    DrawAsciiTree(canvas, child, level + 1, ref y);
                }
            }
        }



        public TreeNode HitTestAI(float x, float y)
        {
            foreach (var kvp in _aiButtonRects)
            {
                if (kvp.Value.Contains(x, y))
                    return kvp.Key;
            }
            return null;
        }

        /// <summary>
        /// Verifică dacă un punct X,Y e peste un nod
        /// </summary>
        public TreeNode HitTest(float x, float y)
        {
            foreach (var kvp in _nodeRects)
            {
                if (kvp.Value.Contains(x, y))
                    return kvp.Key;
            }
            return null;
        }

        private readonly HashSet<TreeNode> _aiButtonClicked = new();

        public void MarkAIButtonClicked(TreeNode node)
        {
            _aiButtonClicked.Add(node);
            _graphics?.Invalidate(); // redesenează pentru a aplica culoarea

            // Resetăm feedback-ul după 300ms
            Task.Delay(300).ContinueWith(_ =>
            {
                _aiButtonClicked.Remove(node);
                _graphics?.Invalidate();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
