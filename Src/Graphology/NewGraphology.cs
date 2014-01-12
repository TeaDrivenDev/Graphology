using Fasterflect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TeaDriven.Graphology
{
    public class GraphNode
    {
        public Type ReferenceType { get; set; }

        public Type ObjectType { get; set; }

        public string ReferenceName { get; set; }

        private IEnumerable<GraphNode> _subGraph = new List<GraphNode>();

        public IEnumerable<GraphNode> SubGraph
        {
            get { return this._subGraph; }
            set { this._subGraph = value; }
        }

        public override string ToString()
        {
            return string.Format("{0} : {1}", ObjectType.Name, ReferenceType.Name);
        }
    }

    public class NullGraphNode : GraphNode
    {
    }

    public interface IGetObjectGraphRepresentation
    {
        GraphNode For(object currentObject, Type referenceType, string referenceName, IEnumerable<object> graphPath);
    }

    public class DefaultGetObjectGraphRepresentation : IGetObjectGraphRepresentation
    {
        private ExclusionRulesSet _exclusionRules = new MinimalExclusionRulesSet();

        public DefaultGetObjectGraphRepresentation(ExclusionRulesSet exclusionRules)
        {
            this.ExclusionRules = exclusionRules;
        }

        private ExclusionRulesSet ExclusionRules
        {
            get { return this._exclusionRules; }

            set
            {
                this._exclusionRules.DoNotFollow = value.DoNotFollow;
                this._exclusionRules.Exclude = value.Exclude;
            }
        }

        public DefaultGetObjectGraphRepresentation()
        {
        }

        #region IGetObjectGraphRepresentation Members

        public GraphNode For(object currentObject, Type referenceType, string referenceName, IEnumerable<object> graphPath)
        {
            GraphNode node = new GraphNode()
                            {
                                ReferenceType = referenceType,
                                ObjectType = currentObject.GetType(),
                                ReferenceName = referenceName,
                            };

            if ((!graphPath.Contains(currentObject)) && (!DoNotFollowType(currentObject.GetType())))
            {
                IList<GraphNode> subGraph = new List<GraphNode>();
                bool handled = GetSubGraph(currentObject, graphPath.Concat(new List<object>() { currentObject }), out subGraph);

                node.SubGraph = subGraph;
            }

            return node;
        }

        private bool GetSubGraph(object currentObject, IEnumerable<object> graphPath, out IList<GraphNode> subGraph)
        {
            subGraph = new List<GraphNode>();

            var type = currentObject.GetType();

            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

            var fieldValues = fields.Select(fi => new { ReferenceType = fi.FieldType, ReferenceName = fi.Name, FieldValue = currentObject.GetFieldValue(fi.Name) });

            foreach (var fieldValue in fieldValues.Where(v => null != v.FieldValue))
            {
                if (!this.TypeIsExcluded(fieldValue.ReferenceType))
                {
                    subGraph.Add(this.For(fieldValue.FieldValue, fieldValue.ReferenceType, fieldValue.ReferenceName, graphPath));
                }
            }

            return true;
        }

        #endregion IGetObjectGraphRepresentation Members

        private bool TypeIsExcluded(Type t)
        {
            return this._exclusionRules.Exclude.AppliesTo(t);
        }

        private bool DoNotFollowType(Type t)
        {
            return this._exclusionRules.DoNotFollow.AppliesTo(t);
        }
    }

    public class GraphVisualizer
    {
        public bool ShowDependencyTypes
        {
            get { return this._showDependencyTypes; }
            set { this._showDependencyTypes = value; }
        }

        private bool _showDependencyTypes = true;

        public string Draw(GraphNode graphNode)
        {
            return this.Draw(graphNode, 0);
        }

        private string Draw(GraphNode graphNode, int depth)
        {
            string graph = this.GetTypeString(graphNode.ReferenceType.Name, depth, graphNode.ObjectType);

            foreach (GraphNode subGraphNode in graphNode.SubGraph)
            {
                int newDepth = depth + 1;

                graph += this.Draw(subGraphNode, newDepth);
            }

            return graph;
        }

        private string GetTypeString(string referenceTypeName, int depth, Type objectType)
        {
            var depthString = "";
            if (depth > 0)
            {
                depthString += new string(' ', 3 * (depth - 1));
                depthString += " >";
            }

            var memberTypeString = this.GetMemberTypeString(referenceTypeName);
            var graph = string.Format("{0} {1}{2}{3}", depthString, objectType.Name, memberTypeString, Environment.NewLine);

            return graph;
        }

        private string GetMemberTypeString(string referenceTypeName)
        {
            var memberTypeString = (this.ShowDependencyTypes
                                        ? (string.IsNullOrEmpty(referenceTypeName) ? "" : " : " + referenceTypeName)
                                        : "");

            return memberTypeString;
        }
    }
}