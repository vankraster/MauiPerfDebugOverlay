using MauiPerfDebugOverlay.Models.Internal;

namespace MauiPerfDebugOverlay.InternalControls
{
    public class TreeDrawable : IDrawable
    {
        private readonly TreeNode _root;
        private const float NodeWidth = 100;
        private const float NodeHeight = 40;
        private const float VerticalSpacing = 70;
        private const float HorizontalSpacing = 30;

        public TreeDrawable(TreeNode root)
        {
            _root = root;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FontSize = 14;
            DrawNode(canvas, _root, dirtyRect.Width / 2, 20);
        }

        private void DrawNode(ICanvas canvas, TreeNode node, float x, float y)
        {
            // Desenăm nodul (un dreptunghi cu text)
            var rect = new RectF(x - NodeWidth / 2, y, NodeWidth, NodeHeight);
            canvas.FillColor = Colors.LightGreen;
            canvas.FillRectangle(rect);

            canvas.StrokeColor = Colors.DarkGreen;
            canvas.DrawRectangle(rect);

            canvas.FontColor = Colors.Black;
            canvas.DrawString(
                node.Name,
                rect,
                HorizontalAlignment.Center,
                VerticalAlignment.Center
            );

            // Desenăm copiii
            if (node.Children.Any())
            {
                float childX = x - (node.Children.Count - 1) * (NodeWidth + HorizontalSpacing) / 2;
                float childY = y + NodeHeight + VerticalSpacing;

                foreach (var child in node.Children)
                {
                    // Linie către copil
                    canvas.StrokeColor = Colors.Gray;
                    canvas.DrawLine(x, y + NodeHeight, childX, childY);

                    // Recursiv copilul
                    DrawNode(canvas, child, childX, childY);

                    childX += NodeWidth + HorizontalSpacing;
                }
            }
        }
    }

}
