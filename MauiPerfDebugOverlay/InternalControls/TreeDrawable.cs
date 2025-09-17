using MauiPerfDebugOverlay.Extensions;
using MauiPerfDebugOverlay.Models.Internal;
using MauiPerfDebugOverlay.Services;
using MauiPerfDebugOverlay.Utils;

namespace MauiPerfDebugOverlay.InternalControls
{
    public class TreeDrawable : IDrawable
    {
        private readonly TreeNode _root;
        internal const float LineHeight = 26;
        private const float StartX = 10;

        // pentru hit testing
        private readonly Dictionary<TreeNode, RectF> _nodeRects = new();

        public TreeDrawable(TreeNode root)
        {
            _root = root;
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

            var totalMs = LoadTimeMetricsStore.Instance.GetValue(node.Id);
            var childrenMs = LoadTimeMetricsStore.Instance.GetSumOfChildrenInMs(node);
            var selfMs = (totalMs.HasValue && childrenMs > 0) ? totalMs - childrenMs : totalMs;

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
            if (selfMs.HasValue)
            {
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
            }
            canvas.FontColor = indicatorColor;
            canvas.DrawString(indicator, new RectF(textX + 150, y, 20, LineHeight),
                              HorizontalAlignment.Left, VerticalAlignment.Top);

            if (selfMs.HasValue)
            {
                // 4️⃣ timp numeric
                canvas.FontColor = Colors.LightGray;
                canvas.DrawString($"{selfMs.FormatTime()}", new RectF(textX + 180, y, 80, LineHeight),
                                  HorizontalAlignment.Left, VerticalAlignment.Top);


                // 5️⃣ bară de progres

                float maxBarWidth = 150;
                float barWidth = Math.Min(maxBarWidth, (float)(selfMs.Value / 1000 * maxBarWidth));
                Color barColor = selfMs > PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.LoadTimeDanger
                    ? Colors.Red
                    : selfMs > PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.LoadTimeWarning
                        ? Colors.Yellow
                        : Colors.Green;

                canvas.FillColor = barColor;
                canvas.FillRectangle(new RectF(textX + 280, y + 4, barWidth, LineHeight - 8));
            }

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

        private void _DrawAsciiTree(ICanvas canvas, TreeNode node, string indent, bool last, ref float y)
        {
            canvas.FontColor = Colors.White;
            string indicator = "";

            var totalMs = LoadTimeMetricsStore.Instance.GetValue(node.Id);
            var childrenMs = LoadTimeMetricsStore.Instance.GetSumOfChildrenInMs(node);
            var selfMs = (totalMs.HasValue && childrenMs > 0)
                ? totalMs - childrenMs
                : totalMs;

            if (selfMs.HasValue)
            {
                // schimbăm culoarea și indicatorul în funcție de praguri
                if (selfMs > PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.LoadTimeDanger)
                {
                    indicator = "⛔";
                    canvas.FontColor = Color.FromHex("D98880");
                }
                else if (selfMs > PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.LoadTimeWarning)
                {
                    indicator = "⚠";
                    canvas.FontColor = Color.FromHex("FFECB3");
                }
                else
                {
                    //indicator = "✔";
                    //canvas.FontColor = Colors.LightGreen;
                }
            }

            string expandSymbol = node.Children.Count > 0
                ? (node.IsExpanded ? " [-] " : " [+] ")
                : " ";

            // compunem linia: nume + timp propriu + total
            string line = $"{indent}{(last ? "└── " : "├── ")}{node.Name}{expandSymbol}{indicator}";

            if (selfMs.HasValue)
            {
                line += $" {selfMs.FormatTime()}";

                if (childrenMs > 0)
                    line += $" (Total {totalMs.FormatTime()})";
            }

            // desenăm textul
            var rect = new RectF(StartX, y - LineHeight / 2, 800, LineHeight);
            _nodeRects[node] = rect;
            canvas.DrawString(line, rect, HorizontalAlignment.Left, VerticalAlignment.Top);

            y += LineHeight;

            // dacă nodul e collapsed, nu desenăm copiii
            if (!node.IsExpanded || node.Children.Count == 0)
                return;

            string childIndent = indent + (last ? "    " : "│   ");

            for (int i = 0; i < node.Children.Count; i++)
            {
                bool isLast = (i == node.Children.Count - 1);
                _DrawAsciiTree(canvas, node.Children[i], childIndent, isLast, ref y);
            }
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
    }
}
