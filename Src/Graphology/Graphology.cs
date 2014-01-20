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

    public interface IGraphologistComponents
    {
        IGraphTraversal GraphTraversal { get; }

        IGraphVisualization GraphVisualization { get; }
    }

    public class GraphologistComponents : IGraphologistComponents
    {
        private IGraphTraversal _graphTraversal;
        private IGraphVisualization _graphVisualization;

        public GraphologistComponents(IGraphTraversal graphTraversal, IGraphVisualization graphVisualization)
        {
            this._graphTraversal = graphTraversal;
            this._graphVisualization = graphVisualization;
        }

        #region IGraphologistComponents Members

        public IGraphTraversal GraphTraversal
        {
            get { return this._graphTraversal; }
        }

        public IGraphVisualization GraphVisualization
        {
            get { return this._graphVisualization; }
        }

        #endregion IGraphologistComponents Members
    }

    public class DefaultGraphologistComponents : GraphologistComponents
    {
        public DefaultGraphologistComponents(TypeExclusions typeExclusions, ITypeFieldExclusion typeFieldExclusion)
            : base(
                new GraphTraversal(BuildGetObjectGraph(typeExclusions, typeFieldExclusion)),
                new GraphVisualization(new DefaultGetNodeString(new DefaultGetDepthString(), new DefaultGetMemberTypesString(BuildGetTypeNameString()))))
        {
        }

        public DefaultGraphologistComponents()
            : this(DefaultTypeExclusions, DefaultTypeFieldExclusion)
        {
        }

        public DefaultGraphologistComponents(TypeExclusions typeExclusions)
            : this(typeExclusions, DefaultTypeFieldExclusion)
        {
        }

        public DefaultGraphologistComponents(ITypeFieldExclusion typeFieldExclusion)
            : this(DefaultTypeExclusions, typeFieldExclusion)
        {
        }

        private static readonly TypeExclusions _defaultTypeExclusions = new MinimalTypeExclusions();
        private static readonly ITypeFieldExclusion _defaultTypeFieldExclusion = new GenericListItemsTypeFieldExclusion();

        public static TypeExclusions DefaultTypeExclusions
        {
            get { return _defaultTypeExclusions; }
        }

        public static ITypeFieldExclusion DefaultTypeFieldExclusion
        {
            get { return _defaultTypeFieldExclusion; }
        }

        private static IGetObjectGraph BuildGetObjectGraph(TypeExclusions typeExclusions, ITypeFieldExclusion typeFieldExclusion)
        {
            LazyGetObjectGraph getObjectGraph = new LazyGetObjectGraph();

            IGetSubGraph getSubGraph = new CompositeGetSubGraph(new List<IGetSubGraph>()
                                                                {
                                                                    new EnumerableGetSubGraph(getObjectGraph, typeExclusions),
                                                                    new DefaultGetSubGraph(getObjectGraph,
                                                                                           new DefaultGetObjectFields(
                                                                                               new FilteringGetTypeFields(
                                                                                                   new DefaultGetTypeFields(),
                                                                                                   typeFieldExclusion)),
                                                                                           typeExclusions)
                                                                });

            getObjectGraph.GetObjectGraph = new DefaultGetObjectGraph(getSubGraph, typeExclusions);

            return getObjectGraph;
        }

        private static IGetTypeNameString BuildGetTypeNameString()
        {
            LazyGetTypeNameString lazyGetTypeNameString = new LazyGetTypeNameString();
            IGetTypeNameString getTypeNameString =
                new CompositeGetTypeNameString(new RecursiveGenericTypeGetTypeNameString(lazyGetTypeNameString),
                                               new DefaultGetTypeNameString());
            lazyGetTypeNameString.GetTypeNameString = getTypeNameString;

            return lazyGetTypeNameString;
        }
    }

    public class CreateGraphologist
    {
        private TypeExclusions _typeExclusions = new TypeExclusions();

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
                                                                        getObjectGraph,
                                                                        new DefaultGetObjectFields(
                                                                            new FilteringGetTypeFields(new DefaultGetTypeFields(),
                                                                                                       new GenericListItemsTypeFieldExclusion
                                                                                                           ())), this.TypeExclusions)
                                                                });

            getObjectGraph.GetObjectGraph = new DefaultGetObjectGraph(getSubGraph, this.TypeExclusions);

            IGraphTraversal traversal = new GraphTraversal(getObjectGraph);

            LazyGetTypeNameString lazyGetTypeNameString = new LazyGetTypeNameString();
            IGetTypeNameString getTypeNameString =
                new CompositeGetTypeNameString(new RecursiveGenericTypeGetTypeNameString(lazyGetTypeNameString),
                                               new DefaultGetTypeNameString());
            lazyGetTypeNameString.GetTypeNameString = getTypeNameString;

            IGraphVisualization visualization =
                new GraphVisualization(new DefaultGetNodeString(new DefaultGetDepthString(),
                                                                new DefaultGetMemberTypesString(lazyGetTypeNameString)));

            Graphologist graphologist = new Graphologist(traversal, visualization);

            return graphologist;
        }
    }

    public interface IGraphologist
    {
        string Graph(object targetObject);
    }

    public class Graphologist : IGraphologist
    {
        private readonly IGraphTraversal _traversal;
        private readonly IGraphVisualization _visualization;

        public Graphologist()
            : this(new DefaultGraphologistComponents())
        {
        }

        public Graphologist(TypeExclusions typeExclusions)
            : this(new DefaultGraphologistComponents(typeExclusions))
        {
        }

        public Graphologist(IGraphologistComponents components)
            : this(components.GraphTraversal, components.GraphVisualization)
        {
        }

        public Graphologist(IGraphTraversal traversal, IGraphVisualization visualization)
        {
            this._traversal = traversal;
            this._visualization = visualization;
        }

        public string Graph(object targetObject)
        {
            string graph = this._visualization.Draw(this._traversal.Traverse(targetObject));

            return graph;
        }
    }

    public static class GraphologistExtensions
    {
        public static void WriteGraph(this IGraphologist graphologist, object targetObject, string projectPath, string graphName)
        {
            var currentDir = Directory.GetCurrentDirectory();

            var fileName = string.Format("{0}_Graph.txt", graphName);
            var fullFilePath = Path.Combine(currentDir, Path.Combine(Path.Combine(@"..\..", projectPath), fileName));

            File.WriteAllText(fullFilePath, graphologist.Graph(targetObject));
        }
    }

    public interface IGraphTraversal
    {
        GraphNode Traverse(object targetObject);
    }

    public class GraphTraversal : IGraphTraversal
    {
        private readonly IGetObjectGraph _getObjectGraph;

        public GraphTraversal(IGetObjectGraph getObjectGraph)
        {
            this._getObjectGraph = getObjectGraph;
        }

        public GraphNode Traverse(object targetObject)
        {
            GraphNode graph = this._getObjectGraph.For(targetObject, targetObject.GetType(), "root", new List<object>());

            return graph;
        }
    }

    #endregion Facade objects

    #region Graph visualization

    public interface IGraphVisualization
    {
        string Draw(GraphNode graphNode);
    }

    public class GraphVisualization : IGraphVisualization
    {
        private readonly IGetNodeString _getNodeString;

        public GraphVisualization(IGetNodeString getNodeString)
        {
            this._getNodeString = getNodeString;
        }

        public string Draw(GraphNode graphNode)
        {
            return this.Draw(graphNode, 0);
        }

        private string Draw(GraphNode graphNode, int depth)
        {
            string graph = this._getNodeString.For(graphNode, depth) + Environment.NewLine;

            depth++;

            foreach (GraphNode subGraphNode in graphNode.SubGraph)
            {
                graph += this.Draw(subGraphNode, depth);
            }

            return graph;
        }
    }

    public interface IGetNodeString
    {
        string For(GraphNode graphNode, int depth);
    }

    public class DefaultGetNodeString : IGetNodeString
    {
        private readonly IGetDepthString _getDepthString;
        private readonly IGetMemberTypesString _getMemberTypesString;

        public DefaultGetNodeString(IGetDepthString getDepthString, IGetMemberTypesString getMemberTypesString)
        {
            if (getDepthString == null) throw new ArgumentNullException("getDepthString");
            if (getMemberTypesString == null) throw new ArgumentNullException("getMemberTypesString");

            this._getDepthString = getDepthString;
            this._getMemberTypesString = getMemberTypesString;
        }

        #region IGetNodeString Members

        public string For(GraphNode graphNode, int depth)
        {
            if (graphNode == null) throw new ArgumentNullException("graphNode");

            string depthString = this._getDepthString.For(depth);
            string memberTypesString = this._getMemberTypesString.For(graphNode);

            string nodeString = string.Format("{0}{1}", depthString, memberTypesString);

            return nodeString;
        }

        #endregion IGetNodeString Members
    }

    public interface IGetDepthString
    {
        string For(int depth);
    }

    public class DefaultGetDepthString : IGetDepthString
    {
        public string For(int depth)
        {
            var depthString = "";
            if (depth > 0)
            {
                depthString += new string(' ', 3 * (depth - 1));
                depthString += " > ";
            }
            return depthString;
        }
    }

    public interface IGetMemberTypesString
    {
        string For(GraphNode graphNode);
    }

    public class DefaultGetMemberTypesString : IGetMemberTypesString
    {
        private readonly IGetTypeNameString _getTypeNameString;

        public DefaultGetMemberTypesString(IGetTypeNameString getTypeNameString)
        {
            if (getTypeNameString == null) throw new ArgumentNullException("getTypeNameString");

            _getTypeNameString = getTypeNameString;
        }

        #region IGetMemberTypesString Members

        public string For(GraphNode graphNode)
        {
            if (graphNode == null) throw new ArgumentNullException("graphNode");

            string objectTypeName;
            this._getTypeNameString.For(graphNode.ObjectType, out objectTypeName);
            string referenceTypeName;
            this._getTypeNameString.For(graphNode.ReferenceType, out referenceTypeName);

            string memberTypesString = string.Format("{0} : {1}", objectTypeName, referenceTypeName);

            return memberTypesString;
        }

        #endregion IGetMemberTypesString Members
    }

    public interface IGetTypeNameString
    {
        bool For(Type type, out string typeName);
    }

    public class LazyGetTypeNameString : IGetTypeNameString
    {
        private IGetTypeNameString _getTypeNameString;

        #region IGetTypeNameString Members

        public bool For(Type type, out string typeName)
        {
            if (type == null) throw new ArgumentNullException("type");

            return this._getTypeNameString.For(type, out typeName);
        }

        #endregion IGetTypeNameString Members

        public IGetTypeNameString GetTypeNameString
        {
            get { return this._getTypeNameString; }
            set { this._getTypeNameString = value; }
        }
    }

    public class CompositeGetTypeNameString : IGetTypeNameString
    {
        private readonly IEnumerable<IGetTypeNameString> _innerInstances;

        public IEnumerable<IGetTypeNameString> InnerInstances
        {
            get { return this._innerInstances; }
        }

        public CompositeGetTypeNameString(params IGetTypeNameString[] innerInstances)
        {
            if (innerInstances == null) throw new ArgumentNullException("innerInstances");

            this._innerInstances = innerInstances;
        }

        public CompositeGetTypeNameString(IEnumerable<IGetTypeNameString> innerInstances)
        {
            if (innerInstances == null) throw new ArgumentNullException("innerInstances");

            this._innerInstances = innerInstances;
        }

        #region IGetTypeNameString Members

        public bool For(Type type, out string typeName)
        {
            if (type == null) throw new ArgumentNullException("type");

            typeName = "";

            bool handled = false;

            foreach (IGetTypeNameString innerInstance in _innerInstances)
            {
                handled = innerInstance.For(type, out typeName);

                if (handled)
                {
                    break;
                }
            }

            return handled;
        }

        #endregion IGetTypeNameString Members
    }

    public class DefaultGetTypeNameString : IGetTypeNameString
    {
        #region IGetTypeNameString Members

        public bool For(Type type, out string typeName)
        {
            if (type == null) throw new ArgumentNullException("type");

            typeName = type.Name;

            return true;
        }

        #endregion IGetTypeNameString Members
    }

    public class RecursiveGenericTypeGetTypeNameString : IGetTypeNameString
    {
        private readonly IGetTypeNameString _innerGetTypeNameString;

        public RecursiveGenericTypeGetTypeNameString(IGetTypeNameString innerGetTypeNameString)
        {
            if (innerGetTypeNameString == null) throw new ArgumentNullException("innerGetTypeNameString");
            this._innerGetTypeNameString = innerGetTypeNameString;
        }

        #region IGetTypeNameString Members

        public bool For(Type type, out string typeName)
        {
            if (type == null) throw new ArgumentNullException("type");

            bool handled = false;

            if (type.IsGenericType)
            {
                typeName = Regex.Match(type.Name, @"^([^`]*)").Value;

                Type[] genericArguments = type.GetGenericArguments();

                IList<string> genericArgumentNames = new List<string>();

                foreach (Type genericArgument in genericArguments)
                {
                    string genericArgumentName;
                    this._innerGetTypeNameString.For(genericArgument, out genericArgumentName);

                    genericArgumentNames.Add(genericArgumentName);
                }

                string genericArgumentsString = string.Join(", ", genericArgumentNames.ToArray());

                typeName = string.Format("{0}<{1}>", typeName, genericArgumentsString);

                handled = true;
            }
            else
            {
                typeName = "";
            }

            return handled;
        }

        #endregion IGetTypeNameString Members
    }

    #endregion Graph visualization

    public abstract class TypeExclusionsClientBase
    {
        protected TypeExclusionsClientBase()
        {
        }

        private readonly TypeExclusions _typeExclusions = new TypeExclusions();

        protected TypeExclusionsClientBase(TypeExclusions typeExclusions)
        {
            if (typeExclusions == null) throw new ArgumentNullException("typeExclusions");

            this._typeExclusions.Add(typeExclusions);
        }

        protected bool TypeIsExcluded(Type t)
        {
            bool applies = this._typeExclusions.Exclude.AppliesTo(t);

            return applies;
        }

        protected bool DoNotFollowType(Type t)
        {
            bool applies = this._typeExclusions.DoNotFollow.AppliesTo(t);

            return applies;
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
            GraphNode graph = this.GetObjectGraph.For(currentObject, referenceType, referenceName, graphPath);

            return graph;
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
            if (currentObject == null) throw new ArgumentNullException("currentObject");
            if (referenceType == null) throw new ArgumentNullException("referenceType");
            if (referenceName == null) throw new ArgumentNullException("referenceName");
            if (graphPath == null) throw new ArgumentNullException("graphPath");

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

        public CompositeGetSubGraph(params IGetSubGraph[] getSubGraphRepresentations)
        {
            _getSubGraphRepresentations = getSubGraphRepresentations;
        }

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
            if (currentObject == null) throw new ArgumentNullException("currentObject");
            if (graphPath == null) throw new ArgumentNullException("graphPath");

            subGraph = new List<GraphNode>();

            bool handled = false;

            var type = currentObject.GetType();

            var genericIenumerable =
                type.GetInterfaces().FirstOrDefault(i => (i.IsGenericType) && (i.GetGenericTypeDefinition() == typeof(IEnumerable<>)));

            if (null != genericIenumerable)
            {
                Type itemType = genericIenumerable.GetGenericArguments().First();

                foreach (var item in currentObject as IEnumerable)
                {
                    if (null != item)
                    {
                        subGraph.Add(this._getObjectGraph.For(item, itemType, "Item", graphPath));
                    }
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
        private readonly IGetObjectFields _getObjectFields;

        public DefaultGetSubGraph(IGetObjectGraph getObjectGraph, IGetObjectFields getObjectFields, TypeExclusions typeExclusions)
            : base(typeExclusions)
        {
            this._getObjectGraph = getObjectGraph;
            _getObjectFields = getObjectFields;
        }

        public bool For(object currentObject, IEnumerable<object> graphPath, out IList<GraphNode> subGraph)
        {
            subGraph = new List<GraphNode>();

            var fieldValues = this._getObjectFields.FieldValues(currentObject);

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

    public interface IGetObjectFields
    {
        IEnumerable<ObjectField> FieldValues(object currentObject);
    }

    public class DefaultGetObjectFields : IGetObjectFields
    {
        private readonly IGetTypeFields _getTypeFields;

        public DefaultGetObjectFields(IGetTypeFields getTypeFields)
        {
            _getTypeFields = getTypeFields;
        }

        public IEnumerable<ObjectField> FieldValues(object currentObject)
        {
            Type type = currentObject.GetType();

            IEnumerable<FieldInfo> fields = this._getTypeFields.For(type);

            IEnumerable<ObjectField> fieldValues = fields.Select(fi => new ObjectField()
                                                                       {
                                                                           ReferenceType = fi.FieldType,
                                                                           ReferenceName = fi.Name,
                                                                           FieldValue = currentObject.GetFieldValue(fi.Name)
                                                                       });

            return fieldValues;
        }
    }

    public interface IGetTypeFields
    {
        IEnumerable<FieldInfo> For(Type type);
    }

    public class FilteringGetTypeFields : IGetTypeFields
    {
        private readonly IGetTypeFields _getTypeFields;
        private readonly ITypeFieldExclusion _typeFieldExclusion;

        public FilteringGetTypeFields(IGetTypeFields getTypeFields, ITypeFieldExclusion typeFieldExclusion)
        {
            this._getTypeFields = getTypeFields;
            this._typeFieldExclusion = typeFieldExclusion;
        }

        #region IGetTypeFields Members

        public IEnumerable<FieldInfo> For(Type type)
        {
            IEnumerable<FieldInfo> fields = this._getTypeFields.For(type).Where(f => !this._typeFieldExclusion.AppliesTo(type, f));

            return fields;
        }

        #endregion IGetTypeFields Members
    }

    public class DefaultGetTypeFields : IGetTypeFields
    {
        public IEnumerable<FieldInfo> For(Type type)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

            return fields;
        }
    }

    public interface ITypeFieldExclusion
    {
        bool AppliesTo(Type type, FieldInfo field);
    }

    public class GenericListItemsTypeFieldExclusion : ITypeFieldExclusion
    {
        public bool AppliesTo(Type type, FieldInfo field)
        {
            bool typeIsGenericList = ((type.IsGenericType) && (typeof(List<>) == type.GetGenericTypeDefinition()));
            bool fieldIsItems = ("_items" == field.Name);

            bool applies = (typeIsGenericList) && (fieldIsItems);

            return applies;
        }
    }

    public class ObjectField
    {
        public Type ReferenceType { get; set; }

        public string ReferenceName { get; set; }

        public object FieldValue { get; set; }
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
            string text = string.Format("{0} : {1}", ObjectType.Name, ReferenceType.Name);

            return text;
        }
    }

    #region Exclusion Rules

    public class TypeExclusions
    {
        protected ITypeExclusion _excludeFixed = new EmptyTypeExclusion();
        private ITypeExclusion _exclude;

        public ITypeExclusion Exclude
        {
            get
            {
                ITypeExclusion value = this._exclude ?? this._excludeFixed;

                return value;
            }

            set
            {
                if (value == null) throw new ArgumentNullException("Exclusion");

                this._exclude = new CompositeTypeExclusion(this._excludeFixed, value);
            }
        }

        protected ITypeExclusion _doNotFollowFixed = new EmptyTypeExclusion();
        private ITypeExclusion _doNotFollow;

        public ITypeExclusion DoNotFollow
        {
            get
            {
                ITypeExclusion value = this._doNotFollow ?? this._doNotFollowFixed;
                return value;
            }

            set
            {
                if (value == null) throw new ArgumentNullException("DoNotFollow");

                this._doNotFollow = new CompositeTypeExclusion(this._doNotFollowFixed, value);
            }
        }

        public TypeExclusions Add(TypeExclusions additionalExclusions)
        {
            this.Exclude = additionalExclusions.Exclude;
            this.DoNotFollow = additionalExclusions.DoNotFollow;

            return this;
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
        private readonly IEnumerable<ITypeExclusion> _exclusions;

        public CompositeTypeExclusion(IEnumerable<ITypeExclusion> exclusions)
        {
            if (exclusions == null) throw new ArgumentNullException("exclusions");

            this._exclusions = exclusions;
        }

        public CompositeTypeExclusion(params ITypeExclusion[] exclusions)
        {
            if (exclusions == null) throw new ArgumentNullException("exclusions");

            this._exclusions = exclusions;
        }

        #region ITypeExclusion Members

        public bool AppliesTo(Type type)
        {
            return this._exclusions.Any(rule => rule.AppliesTo(type));
        }

        #endregion ITypeExclusion Members

        public IEnumerable<ITypeExclusion> TypeExclusions
        {
            get
            {
                return this._exclusions;
            }
        }
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
        private readonly string _ns;

        public ExactNamespaceTypeExclusion(string @namespace)
            : base(type =>
                   {
                       @namespace = Regex.Replace(@namespace, @"\.$", "");

                       Regex rx = new Regex(string.Format(@"^{0}.[^\.]*$", @namespace));
                       bool applies = rx.IsMatch(type.FullName);

                       return applies;
                   })
        {
            _ns = @namespace;
        }

        public string Namespace
        {
            get { return this._ns; }
        }
    }

    public class RootNamespaceTypeExclusion : FuncTypeExclusion
    {
        private readonly string _ns;

        public RootNamespaceTypeExclusion(string @namespace)
            : base(type =>
                   {
                       @namespace = Regex.Replace(@namespace, @"\.$", "");
                       bool applies = type.FullName.StartsWith(string.Format("{0}.", @namespace));

                       return applies;
                   })
        {
            _ns = @namespace;
        }

        public string Namespace
        {
            get { return this._ns; }
        }
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
            bool applies = this._types.Contains(type);

            return applies;
        }

        #endregion TypeExclusion Members

        public IEnumerable<Type> Types
        {
            get
            {
                return this._types;
            }
        }
    }

    #endregion Exclusion Rules
}