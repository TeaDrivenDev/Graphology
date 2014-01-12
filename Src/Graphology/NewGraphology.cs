using Fasterflect;
using System;
using System.Collections;
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

    public class CreateNewGraphologist
    {
        private bool _showDependencyTypes = true;
        private ExclusionRulesSet _exclusionRules = new ExclusionRulesSet();

        public bool ShowDependencyTypes
        {
            get { return this._showDependencyTypes; }
            set { this._showDependencyTypes = value; }
        }

        public ExclusionRulesSet ExclusionRules
        {
            get { return this._exclusionRules; }
            set { this._exclusionRules = value; }
        }

        public NewGraphologist Now()
        {
            LazyGetObjectGraphRepresentation getObjectGraph = new LazyGetObjectGraphRepresentation();

            IGetSubGraphRepresentation getSubGraph = new CompositeGetSubGraphRepresentation(new List<IGetSubGraphRepresentation>()
                                                                {
                                                                    new EnumerableGetSubGraphRepresentation(getObjectGraph, this.ExclusionRules),
                                                                    new DefaultGetSubGraphRepresentation(getObjectGraph, this.ExclusionRules)
                                                                });

            getObjectGraph.GetObjectGraphRepresentation = new DefaultGetObjectGraphRepresentation(getSubGraph, this.ExclusionRules);

            GraphTraversal traversal = new GraphTraversal(getObjectGraph);

            GraphVisualizer visualizer = new GraphVisualizer();

            NewGraphologist graphologist = new NewGraphologist(traversal, visualizer);

            return graphologist;
        }
    }

    public class NewGraphologist
    {
        private readonly GraphTraversal _traversal;
        private readonly GraphVisualizer _visualizer;

        public NewGraphologist(GraphTraversal traversal, GraphVisualizer visualizer)
        {
            _traversal = traversal;
            _visualizer = visualizer;
        }

        public string Graph(object targetObject)
        {
            return this._visualizer.Draw(this._traversal.Traverse(targetObject));
        }
    }

    public class GraphTraversal
    {
        private readonly IGetObjectGraphRepresentation _getObjectGraphRepresentation;

        public GraphTraversal(IGetObjectGraphRepresentation getObjectGraphRepresentation)
        {
            _getObjectGraphRepresentation = getObjectGraphRepresentation;
        }

        public GraphNode Traverse(object targetObject)
        {
            return this._getObjectGraphRepresentation.For(targetObject, targetObject.GetType(), "root", new List<object>());
        }
    }

    public interface IGetObjectGraphRepresentation
    {
        GraphNode For(object currentObject, Type referenceType, string referenceName, IEnumerable<object> graphPath);
    }

    public class LazyGetObjectGraphRepresentation : IGetObjectGraphRepresentation
    {
        public IGetObjectGraphRepresentation GetObjectGraphRepresentation { get; set; }

        #region IGetObjectGraphRepresentation Members

        public GraphNode For(object currentObject, Type referenceType, string referenceName, IEnumerable<object> graphPath)
        {
            return this.GetObjectGraphRepresentation.For(currentObject, referenceType, referenceName, graphPath);
        }

        #endregion IGetObjectGraphRepresentation Members
    }

    public class DefaultGetObjectGraphRepresentation : ExclusionRuleClientBase, IGetObjectGraphRepresentation
    {
        private readonly IGetSubGraphRepresentation _getSubGraphRepresentation;

        public DefaultGetObjectGraphRepresentation(IGetSubGraphRepresentation getSubGraphRepresentation, ExclusionRulesSet exclusionRules)
            : base(exclusionRules)
        {
            _getSubGraphRepresentation = getSubGraphRepresentation;
        }

        public DefaultGetObjectGraphRepresentation(IGetSubGraphRepresentation getSubGraphRepresentation)
        {
            _getSubGraphRepresentation = getSubGraphRepresentation;
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
                bool handled = this._getSubGraphRepresentation.For(currentObject, graphPath.Concat(new List<object>() { currentObject }), out subGraph);

                node.SubGraph = subGraph;
            }

            return node;
        }

        #endregion IGetObjectGraphRepresentation Members
    }

    public abstract class ExclusionRuleClientBase
    {
        protected ExclusionRuleClientBase()
        {
        }

        private ExclusionRulesSet _exclusionRules = new MinimalExclusionRulesSet();

        protected ExclusionRuleClientBase(ExclusionRulesSet exclusionRules)
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

        protected bool TypeIsExcluded(Type t)
        {
            return this._exclusionRules.Exclude.AppliesTo(t);
        }

        protected bool DoNotFollowType(Type t)
        {
            return this._exclusionRules.DoNotFollow.AppliesTo(t);
        }
    }

    public interface IGetSubGraphRepresentation
    {
        bool For(object currentObject, IEnumerable<object> graphPath, out IList<GraphNode> subGraph);
    }

    public class CompositeGetSubGraphRepresentation : IGetSubGraphRepresentation
    {
        private readonly IEnumerable<IGetSubGraphRepresentation> _getSubGraphRepresentations;

        public CompositeGetSubGraphRepresentation(IEnumerable<IGetSubGraphRepresentation> getSubGraphRepresentations)
        {
            _getSubGraphRepresentations = getSubGraphRepresentations;
        }

        #region IGetSubGraphRepresentation Members

        public bool For(object currentObject, IEnumerable<object> graphPath, out IList<GraphNode> subGraph)
        {
            List<GraphNode> localSubGraph = new List<GraphNode>();

            bool handled = false;

            foreach (IGetSubGraphRepresentation getSubGraph in this._getSubGraphRepresentations)
            {
                IList<GraphNode> subSubGraph;

                handled = getSubGraph.For(currentObject, graphPath, out subSubGraph);

                localSubGraph.AddRange(subSubGraph);

                //if (handled)
                //{
                //    // TODO: Unbreak
                //    break;
                //}
            }

            subGraph = localSubGraph;

            return handled;
        }

        #endregion IGetSubGraphRepresentation Members
    }

    public class EnumerableGetSubGraphRepresentation : ExclusionRuleClientBase, IGetSubGraphRepresentation
    {
        private readonly IGetObjectGraphRepresentation _getObjectGraphRepresentation;

        public EnumerableGetSubGraphRepresentation(IGetObjectGraphRepresentation getObjectGraphRepresentation)
        {
            _getObjectGraphRepresentation = getObjectGraphRepresentation;
        }

        public EnumerableGetSubGraphRepresentation(IGetObjectGraphRepresentation getObjectGraphRepresentation, ExclusionRulesSet exclusionRules)
            : base(exclusionRules)
        {
            _getObjectGraphRepresentation = getObjectGraphRepresentation;
        }

        #region IGetSubGraphRepresentation Members

        public bool For(object currentObject, IEnumerable<object> graphPath, out IList<GraphNode> subGraph)
        {
            subGraph = new List<GraphNode>();

            bool handled = false;

            var enumerable = currentObject as IEnumerable;

            if (null != enumerable)
            {
                foreach (var item in enumerable)
                {
                    subGraph.Add(this._getObjectGraphRepresentation.For(item, typeof(object), "Item", graphPath));
                }

                handled = true;
            }

            return handled;
        }

        #endregion IGetSubGraphRepresentation Members
    }

    public class DefaultGetSubGraphRepresentation : ExclusionRuleClientBase, IGetSubGraphRepresentation
    {
        private readonly IGetObjectGraphRepresentation _getObjectGraphRepresentation;

        public DefaultGetSubGraphRepresentation(IGetObjectGraphRepresentation getObjectGraphRepresentation)
        {
            _getObjectGraphRepresentation = getObjectGraphRepresentation;
        }

        public DefaultGetSubGraphRepresentation(IGetObjectGraphRepresentation getObjectGraphRepresentation, ExclusionRulesSet exclusionRules)
            : base(exclusionRules)
        {
            _getObjectGraphRepresentation = getObjectGraphRepresentation;
        }

        public bool For(object currentObject, IEnumerable<object> graphPath, out IList<GraphNode> subGraph)
        {
            subGraph = new List<GraphNode>();

            var type = currentObject.GetType();

            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

            var fieldValues = fields.Select(fi => new { ReferenceType = fi.FieldType, ReferenceName = fi.Name, FieldValue = currentObject.GetFieldValue(fi.Name) });

            foreach (var fieldValue in fieldValues.Where(v => null != v.FieldValue))
            {
                if (!this.TypeIsExcluded(fieldValue.ReferenceType))
                {
                    subGraph.Add(this._getObjectGraphRepresentation.For(fieldValue.FieldValue, fieldValue.ReferenceType, fieldValue.ReferenceName, graphPath));
                }
            }

            return true;
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