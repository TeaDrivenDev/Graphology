using System.Collections.Generic;

namespace TeaDriven.Graphology
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<GraphNode> Traverse(this GraphNode root)
        {
            var stack = new Stack<GraphNode>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current;
                foreach (var child in current.SubGraph)
                    stack.Push(child);
            }
        }
    }
}