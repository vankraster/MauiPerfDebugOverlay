using MauiPerfDebugOverlay.Models.Internal;

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
                HeightRequest = 600,
                WidthRequest = 800
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
    }

}
