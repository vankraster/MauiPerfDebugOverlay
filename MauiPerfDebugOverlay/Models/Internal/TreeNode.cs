using MauiPerfDebugOverlay.Services;
using System.Text;

namespace MauiPerfDebugOverlay.Models.Internal
{
    public class TreeNode
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Name { get; set; } = string.Empty;
        public List<TreeNode> Children { get; set; } = new();
        public Dictionary<string, string>? Properties { get; set; }  // aici salvăm proprietățile

        public bool IsExpanded { get; set; } = true; // default: expandat
        public bool ArePropertiesExpanded { get; set; } = false;



        public static string SerializeTree(TreeNode node, int level = 0)
        {
            var indent = new string(' ', level * 2);
            var sb = new StringBuilder();


            var selfMs = LoadTimeMetricsStore.Instance.GetSelfMsByNode(node);
            sb.AppendLine($"{indent}- Node: {node.Name} | Time till HandlerChanged: {selfMs} ms");
            //sb.AppendLine($"{indent}  Time till HandlerChanged in Ms: {selfMs} ms");

            if (node.Children != null && node.Children.Any())
            {
                sb.AppendLine($"{indent}  Children:");
                foreach (var child in node.Children)
                {
                    sb.Append(SerializeTree(child, level + 1));
                }
            }

            return sb.ToString();
        }
    }
}
