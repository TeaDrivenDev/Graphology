using System.IO;
using TeaDriven.Graphology.Traversal;
using XMindAPI.LIB;

namespace TeaDriven.Graphology
{
    public static class GraphologistExtensions
    {
        public static void WriteGraph(this IGraphologist graphologist, object targetObject, string projectPath, string graphName)
        {
            var currentDir = Directory.GetCurrentDirectory();

            var fileName = string.Format("{0}.graph", graphName);
            var fullFilePath = Path.Combine(currentDir, Path.Combine(Path.Combine(@"..\..", projectPath), fileName));

            File.WriteAllText(fullFilePath, graphologist.Graph(targetObject));
        }

        public static void DrawXmind(this IGraphologist graphologist, object targetObject, string projectPath, string graphName)
        {
            IGraphTraversal graphTraversal = new DefaultGraphTraversal();
            var graphRootNode = graphTraversal.Traverse(targetObject);

            var workbook = new XMindWorkBook(projectPath + @"\" + graphName + ".xmind");
            var sheetId = workbook.AddSheet("Graphology");
            var central = workbook.AddCentralTopic(sheetId, "Main", XMindStructure.Map);

            AddXMindTopics(workbook, central, graphRootNode);

            workbook.Save();
        }

        private static void AddXMindTopics(XMindWorkBook workbook, string parentId, GraphNode node)
        {
            var thisId = workbook.AddTopic(parentId, node.ObjectType.Name);

            foreach (var subNode in node.SubGraph)
            {
                AddXMindTopics(workbook, thisId, subNode);
            }
        }
    }
}