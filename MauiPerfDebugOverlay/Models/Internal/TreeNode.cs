namespace MauiPerfDebugOverlay.Models.Internal
{
    public class TreeNode
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Name { get; set; } = string.Empty;
        public List<TreeNode> Children { get; set; } = new();
        public bool IsExpanded { get; set; } = true; // default: expandat

    }
}
