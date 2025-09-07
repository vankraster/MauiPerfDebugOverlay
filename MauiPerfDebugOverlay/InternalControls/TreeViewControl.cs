using MauiPerfDebugOverlay.Models.Internal;
using Microsoft.Maui.Controls;

namespace MauiPerfDebugOverlay.InternalControls
{
    public class TreeViewControl : ContentView
    {
        private readonly GraphicsView _graphicsView;

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
                HeightRequest = 1600,
                WidthRequest = 800
            };

            _graphicsView.StartInteraction += (s, e) =>
            {
                Tapped(e.Touches[0].X, e.Touches[0].Y);
            };

            Content = new ScrollView
            {
                Content = _graphicsView
            };
        }

        private static void OnRootNodeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is TreeViewControl control && newValue is TreeNode root)
            {
                control._graphicsView.Drawable = new TreeDrawable(root);
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
                    _graphicsView.Invalidate();
                }
            }
        }
    }

}
