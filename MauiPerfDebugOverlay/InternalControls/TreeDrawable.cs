using MauiPerfDebugOverlay.Extensions;
using MauiPerfDebugOverlay.Models.Internal;
using MauiPerfDebugOverlay.Services;
using MauiPerfDebugOverlay.Utils;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;

namespace MauiPerfDebugOverlay.InternalControls
{
    public class TreeDrawable : IDrawable
    {
        private readonly TreeNode _root;
        internal const float LineHeight = 22;
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
            DrawAsciiTree(canvas, _root, "", true, ref y);
        }

        private void DrawAsciiTree(ICanvas canvas, TreeNode node, string indent, bool last, ref float y)
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
                    canvas.FontColor = Colors.IndianRed;
                }
                else if (selfMs > PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.LoadTimeWarning)
                {
                    indicator = "⚠";
                    canvas.FontColor = Colors.Gold;
                }
                else
                {
                    indicator = "✔";
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
                DrawAsciiTree(canvas, node.Children[i], childIndent, isLast, ref y);
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
