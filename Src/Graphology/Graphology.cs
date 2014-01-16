using Fasterflect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TeaDriven.Graphology
{
    #region Facade objects

    public class CreateGraphologist
    {
        private bool _showDependencyTypes = true;
        private TypeExclusions _typeExclusions = new TypeExclusions();

        public bool ShowDependencyTypes
        {
            get { return this._showDependencyTypes; }
            set { this._showDependencyTypes = value; }
        }

        public TypeExclusions TypeExclusions
        {
            get { return this._typeExclusions; }
            set { this._typeExclusions = value; }
        }

        public Graphologist Now()
        {
            LazyGetObjectGraph getObjectGraph = new LazyGetObjectGraph();

            IGetSubGraph getSubGraph = new CompositeGetSubGraph(new List<IGetSubGraph>()
                                                                                            {
                                                                                                new EnumerableGetSubGraph(
                                                                                                    getObjectGraph, this.TypeExclusions),
                                                                                                new DefaultGetSubGraph(
                                                                                                    getObjectGraph, this.TypeExclusions)
                                                                                            });

            getObjectGraph.GetObjectGraph = new DefaultGetObjectGraph(getSubGraph, this.TypeExclusions);

            GraphTraversal traversal = new GraphTraversal(getObjectGraph);

            GraphVisualizer visualizer = new GraphVisualizer();

            Graphologist graphologist = new Graphologist(traversal, visualizer);

            return graphologist;
        }
    }

    public class Graphologist
    {
        private readonly GraphTraversal _traversal;
        private readonly GraphVisualizer _visualizer;

        public Graphologist(GraphTraversal traversal, GraphVisualizer visualizer)
        {
            _traversal = traversal;
            _visualizer = visualizer;
        }

        public string Graph(object targetObject)
        {
            return this._visualizer.Draw(this._traversal.Traverse(targetObject));
        }
    }

    public static class GraphologistExtensions
    {
        public static void WriteGraph(this Graphologist graphologist, object targetObject, string projectPath, string graphName)
        {
            var currentDir = Directory.GetCurrentDirectory();

            var fileName = string.Format("{0}_Graph.txt", graphName);
            var fullFilePath = Path.Combine(currentDir, Path.Combine(Path.Combine(@"..\..", projectPath), fileName));

            File.WriteAllText(fullFilePath, graphologist.Graph(targetObject));
        }
    }

    public class GraphTraversal
    {
        private readonly IGetObjectGraph _getObjectGraph;

        public GraphTraversal(IGetObjectGraph getObjectGraph)
        {
            this._getObjectGraph = getObjectGraph;
        }

        public GraphNode Traverse(object targetObject)
        {
            return this._getObjectGraph.For(targetObject, targetObject.GetType(), "root", new List<object>());
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

    #endregion Facade objects

    public abstract class TypeExclusionsClientBase
    {
        protected TypeExclusionsClientBase()
        {
        }

        private TypeExclusions _typeExclusions = new MinimalTypeExclusions();

        protected TypeExclusionsClientBase(TypeExclusions typeExclusions)
        {
            if (typeExclusions == null) throw new ArgumentNullException("typeExclusions");

            this.TypeExclusions = typeExclusions;
        }

        private TypeExclusions TypeExclusions
        {
            get { return this._typeExclusions; }

            set
            {
                this._typeExclusions.DoNotFollow = value.DoNotFollow;
                this._typeExclusions.Exclude = value.Exclude;
            }
        }

        protected bool TypeIsExcluded(Type t)
        {
            return this._typeExclusions.Exclude.AppliesTo(t);
        }

        protected bool DoNotFollowType(Type t)
        {
            return this._typeExclusions.DoNotFollow.AppliesTo(t);
        }
    }

    #region IGetObjectGraph

    public interface IGetObjectGraph
    {
        GraphNode For(object currentObject, Type referenceType, string referenceName, IEnumerable<object> graphPath);
    }

    public class LazyGetObjectGraph : IGetObjectGraph
    {
        public IGetObjectGraph GetObjectGraph { get; set; }

        #region IGetObjectGraph Members

        public GraphNode For(object currentObject, Type referenceType, string referenceName, IEnumerable<object> graphPath)
        {
            return this.GetObjectGraph.For(currentObject, referenceType, referenceName, graphPath);
        }

        #endregion IGetObjectGraph Members
    }

    public class DefaultGetObjectGraph : TypeExclusionsClientBase, IGetObjectGraph
    {
        private readonly IGetSubGraph _getSubGraph;

        public DefaultGetObjectGraph(IGetSubGraph getSubGraph, TypeExclusions typeExclusions)
            : base(typeExclusions)
        {
            if (getSubGraph == null) throw new ArgumentNullException("getSubGraph");

            this._getSubGraph = getSubGraph;
        }

        public DefaultGetObjectGraph(IGetSubGraph getSubGraph)
        {
            if (getSubGraph == null) throw new ArgumentNullException("getSubGraph");

            this._getSubGraph = getSubGraph;
        }

        #region IGetObjectGraph Members

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
                bool handled = this._getSubGraph.For(currentObject, graphPath.Concat(new List<object>() { currentObject }),
                                                                   out subGraph);

                node.SubGraph = subGraph;
            }

            return node;
        }

        #endregion IGetObjectGraph Members
    }

    #endregion IGetObjectGraph

    #region IGetSubGraph

    public interface IGetSubGraph
    {
        bool For(object currentObject, IEnumerable<object> graphPath, out IList<GraphNode> subGraph);
    }

    public class CompositeGetSubGraph : IGetSubGraph
    {
        private readonly IEnumerable<IGetSubGraph> _getSubGraphRepresentations;

        public CompositeGetSubGraph(IEnumerable<IGetSubGraph> getSubGraphRepresentations)
        {
            _getSubGraphRepresentations = getSubGraphRepresentations;
        }

        #region IGetSubGraph Members

        public bool For(object currentObject, IEnumerable<object> graphPath, out IList<GraphNode> subGraph)
        {
            List<GraphNode> localSubGraph = new List<GraphNode>();

            bool handled = false;

            foreach (IGetSubGraph getSubGraph in this._getSubGraphRepresentations)
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

        #endregion IGetSubGraph Members
    }

    public class EnumerableGetSubGraph : TypeExclusionsClientBase, IGetSubGraph
    {
        private readonly IGetObjectGraph _getObjectGraph;

        public EnumerableGetSubGraph(IGetObjectGraph getObjectGraph)
        {
            this._getObjectGraph = getObjectGraph;
        }

        public EnumerableGetSubGraph(IGetObjectGraph getObjectGraph, TypeExclusions typeExclusions)
            : base(typeExclusions)
        {
            this._getObjectGraph = getObjectGraph;
        }

        #region IGetSubGraph Members

        public bool For(object currentObject, IEnumerable<object> graphPath, out IList<GraphNode> subGraph)
        {
            subGraph = new List<GraphNode>();

            bool handled = false;

            var enumerable = currentObject as IEnumerable;

            if (null != enumerable)
            {
                foreach (var item in enumerable)
                {
                    subGraph.Add(this._getObjectGraph.For(item, typeof(object), "Item", graphPath));
                }

                handled = true;
            }

            return handled;
        }

        #endregion IGetSubGraph Members
    }

    public class DefaultGetSubGraph : TypeExclusionsClientBase, IGetSubGraph
    {
        private readonly IGetObjectGraph _getObjectGraph;

        public DefaultGetSubGraph(IGetObjectGraph getObjectGraph)
        {
            this._getObjectGraph = getObjectGraph;
        }

        public DefaultGetSubGraph(IGetObjectGraph getObjectGraph, TypeExclusions typeExclusions)
            : base(typeExclusions)
        {
            this._getObjectGraph = getObjectGraph;
        }

        public bool For(object currentObject, IEnumerable<object> graphPath, out IList<GraphNode> subGraph)
        {
            subGraph = new List<GraphNode>();

            var type = currentObject.GetType();

            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

            var fieldValues =
                fields.Select(
                    fi => new { ReferenceType = fi.FieldType, ReferenceName = fi.Name, FieldValue = currentObject.GetFieldValue(fi.Name) });

            foreach (var fieldValue in fieldValues.Where(v => null != v.FieldValue))
            {
                if (!this.TypeIsExcluded(fieldValue.ReferenceType))
                {
                    subGraph.Add(this._getObjectGraph.For(fieldValue.FieldValue, fieldValue.ReferenceType, fieldValue.ReferenceName,
                                                          graphPath));
                }
            }

            return true;
        }
    }

    #endregion IGetSubGraph

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
            // This is only to see stuff more easily in the debug window; it is too unflexible for practical use
            return string.Format("{0} : {1}", ObjectType.Name, ReferenceType.Name);
        }
    }

    #region Exclusion Rules

    public class TypeExclusions
    {
        protected ITypeExclusion _excludeFixed = new EmptyTypeExclusion();
        private ITypeExclusion _exclude = new EmptyTypeExclusion();

        public ITypeExclusion Exclude
        {
            get { return this._exclude; }
            set
            {
                if (value == null) throw new ArgumentNullException("Exclusion");

                this._exclude = new CompositeTypeExclusion(this._excludeFixed, value);
            }
        }

        protected ITypeExclusion _doNotFollowFixed = new EmptyTypeExclusion();
        private ITypeExclusion _doNotFollow = new EmptyTypeExclusion();

        public ITypeExclusion DoNotFollow
        {
            get { return this._doNotFollow; }
            set
            {
                if (value == null) throw new ArgumentNullException("DoNotFollow");

                this._doNotFollow = new CompositeTypeExclusion(this._doNotFollowFixed, value);
            }
        }
    }

    public class MinimalTypeExclusions : TypeExclusions
    {
        public MinimalTypeExclusions()
        {
            this._doNotFollowFixed = new ExactNamespaceTypeExclusion("System");
        }
    }

    public class TestTypeExclusions : TypeExclusions
    {
        public TestTypeExclusions()
        {
            this._doNotFollowFixed = new RootNamespaceTypeExclusion("Castle.Proxies");
        }
    }

    public interface ITypeExclusion
    {
        bool AppliesTo(Type type);
    }

    public class EmptyTypeExclusion : ITypeExclusion
    {
        #region TypeExclusion Members

        public bool AppliesTo(Type type)
        {
            return false;
        }

        #endregion TypeExclusion Members
    }

    public class CompositeTypeExclusion : ITypeExclusion
    {
        private readonly IEnumerable<ITypeExclusion> _exclusionRules;

        public CompositeTypeExclusion(IEnumerable<ITypeExclusion> exclusionRules)
        {
            if (exclusionRules == null) throw new ArgumentNullException("exclusionRules");

            _exclusionRules = exclusionRules;
        }

        public CompositeTypeExclusion(params ITypeExclusion[] exclusionRules)
        {
            if (exclusionRules == null) throw new ArgumentNullException("exclusionRules");

            _exclusionRules = exclusionRules;
        }

        #region TypeExclusion Members

        public bool AppliesTo(Type type)
        {
            return this._exclusionRules.Any(rule => rule.AppliesTo(type));
        }

        #endregion TypeExclusion Members
    }

    public class FuncTypeExclusion : ITypeExclusion
    {
        private readonly Func<Type, bool> _rule;

        public FuncTypeExclusion(Func<Type, bool> rule)
        {
            if (rule == null) throw new ArgumentNullException("rule");

            _rule = rule;
        }

        #region TypeExclusion Members

        public bool AppliesTo(Type type)
        {
            return this._rule(type);
        }

        #endregion TypeExclusion Members
    }

    public class ExactNamespaceTypeExclusion : FuncTypeExclusion
    {
        public ExactNamespaceTypeExclusion(string @namespace)
            : base(type =>
                   {
                       @namespace = Regex.Replace(@namespace, @"\.$", "");

                       Regex rx = new Regex(string.Format(@"^{0}.[^\.]*", @namespace));

                       return rx.IsMatch(type.FullName);
                   }) { }
    }

    public class RootNamespaceTypeExclusion : FuncTypeExclusion
    {
        public RootNamespaceTypeExclusion(string @namespace)
            : base(type =>
                   {
                       @namespace = Regex.Replace(@namespace, @"\.$", "");

                       return type.FullName.StartsWith(string.Format("{0}.", @namespace));
                   }) { }
    }

    public class ConcreteTypeExclusion : ITypeExclusion
    {
        private readonly IEnumerable<Type> _types;

        public ConcreteTypeExclusion(params Type[] types)
        {
            if (types == null) throw new ArgumentNullException("types");

            _types = types;
        }

        public ConcreteTypeExclusion(IEnumerable<Type> types)
        {
            if (types == null) throw new ArgumentNullException("types");

            _types = types;
        }

        #region TypeExclusion Members

        public bool AppliesTo(Type type)
        {
            return this._types.Contains(type);
        }

        #endregion TypeExclusion Members
    }

    #endregion Exclusion Rules
}