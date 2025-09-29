using MauiPerfDebugOverlay.Extensions;
using MauiPerfDebugOverlay.Models.Internal;
using MauiPerfDebugOverlay.Services;

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
                var treeDrawable = new TreeDrawable(control._graphicsView, root);
                control._graphicsView.Drawable = treeDrawable;
                control._graphicsView.HeightRequest = control.NewHeightRequest;

                control._graphicsView.Invalidate(); // redesenează
            }
        }


        public void Tapped(float x, float y)
        {
            if (_graphicsView.Drawable is TreeDrawable drawable)
            {
                TreeNode clickedNode = null;
                if (PerformanceDebugOverlayExtensions.PerformanceOverlayOptions.ViewTabAI)
                {
                    clickedNode = drawable.HitTestAI(x, y);
                    if (clickedNode != null)
                    {
                        // 🔹 Feedback vizual: colorăm butonul temporar
                        drawable.MarkAIButtonClicked(clickedNode);

                        GeminiService.Instance.AskForTreeNode(clickedNode);
                        return;
                    }
                }



                clickedNode = drawable.HitTest(x, y);
                if (clickedNode != null)
                {
                    clickedNode.IsExpanded = !clickedNode.IsExpanded;

                    _graphicsView.HeightRequest = NewHeightRequest;

                    _graphicsView.Invalidate();

                    return;
                }


                clickedNode = drawable.HitTestProps(x, y);
                if (clickedNode != null)
                {
                    clickedNode.ArePropertiesExpanded = !clickedNode.ArePropertiesExpanded;

                    _graphicsView.HeightRequest = NewHeightRequest;

                    _graphicsView.Invalidate();

                    return;
                }
            }
        }



        double NewHeightRequest => (1 + CountVisibleRows(RootNode)) * TreeDrawable.LineHeight;

        public int CountVisibleRows(TreeNode node)
        {
            int count = 2; // nodul curent

            if (node.ArePropertiesExpanded)
                count += node.Properties?.Count() ?? 0;

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
