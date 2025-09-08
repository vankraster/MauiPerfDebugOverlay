using MauiPerfDebugOverlay.Extensions;
using MauiPerfDebugOverlay.Models.Internal;
using MauiPerfDebugOverlay.Services;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;

namespace MauiPerfDebugOverlay.InternalControls
{
    public class TreeDrawable : IDrawable
    {
        private readonly TreeNode _root;
        internal const float LineHeight = 20;
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
            string slowNodeIndicator = " -> ";
            var loadTimeInMs = LoadTimeMetricsStore.Instance.GetValue(node.Id);
            if (loadTimeInMs.HasValue)
            {

                if (loadTimeInMs > PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.LoadTimeDanger)
                    slowNodeIndicator = "⛔ ";
                else if (loadTimeInMs > PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.LoadTimeWarning)
                    slowNodeIndicator = "⚠ ";
                 

                slowNodeIndicator += loadTimeInMs >= 1100
                                         ? $"{loadTimeInMs / 1000:F2} s" // convert to seconds
                                         : $"{loadTimeInMs:F0} ms";      // keep in milliseconds
            }

            string expandSymbol = "";
            if (node.Children.Count > 0)
                expandSymbol = node.IsExpanded ? " [-] " : " [+] ";


            // prefix pentru nodul curent
            string prefix = indent + (last ? "└── " : "├── ");
            string text = prefix + node.Name + expandSymbol + slowNodeIndicator;

            // desenăm textul
            var rect = new RectF(StartX, y - LineHeight / 2, 500, LineHeight);
            _nodeRects[node] = rect;
            canvas.DrawString(text, rect, HorizontalAlignment.Left, VerticalAlignment.Top);

            y += LineHeight;

            // dacă nodul e collapsed, nu desenăm copiii
            if (!node.IsExpanded || node.Children.Count == 0)
                return;

            // calculăm indentul pentru copii
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
