namespace MauiPerfDebugOverlay.Models.Internal
{
    public class TreeNode
    {
        public string Name { get; set; } = string.Empty;
        public List<TreeNode> Children { get; set; } = new();
    }
}
