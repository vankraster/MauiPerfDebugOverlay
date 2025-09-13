using MauiPerfDebugOverlay.Models.Internal;

namespace MauiPerfDebugOverlay.InternalControls
{
    public class TreeViewControl : ContentView
    {
        internal readonly GraphicsView _graphicsView;

        public static readonly BindableProperty RootNodeProperty =
            BindableProperty.Create(
                nameof(RootNode),
                typeof(TreeNode),
                typeof(TreeViewControl),
                propertyChanged: OnRootNodeChanged);

        public TreeNode RootNode
        {
            get => (TreeNode)GetValue(RootNodeProperty);
            set => SetValue(RootNodeProperty, value);
        }

        public TreeViewControl()
        {
            _graphicsView = new GraphicsView
            {
                HeightRequest = 180,
                WidthRequest = 800,
                VerticalOptions = LayoutOptions.Start, 
            };

            _graphicsView.StartInteraction += (s, e) =>
            {
                Tapped(e.Touches[0].X, e.Touches[0].Y);
            };

            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Both,
                Content = _graphicsView
            };
        }

        private static void OnRootNodeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is TreeViewControl control && newValue is TreeNode root)
            {
                var treeDrawable = new TreeDrawable(root);
                control._graphicsView.Drawable = treeDrawable;
                control._graphicsView.HeightRequest = (1 + control.CountVisibleRows(root)) * TreeDrawable.LineHeight;

                control._graphicsView.Invalidate(); // redesenează
            }
        }


        public void Tapped(float x, float y)
        {
            if (_graphicsView.Drawable is TreeDrawable drawable)
            {
                var clickedNode = drawable.HitTest(x, y);
                if (clickedNode != null)
                {
                    clickedNode.IsExpanded = !clickedNode.IsExpanded;

                    _graphicsView.HeightRequest = (1 + CountVisibleRows(RootNode)) * TreeDrawable.LineHeight;

                    _graphicsView.Invalidate();
                }
            }
        }

        public int CountVisibleRows(TreeNode node)
        {
            int count = 1; // nodul curent

            if (node.IsExpanded)
            {
                foreach (var child in node.Children)
                {
                    count += CountVisibleRows(child);
                }
            }

            return count;
        }
    }

}
