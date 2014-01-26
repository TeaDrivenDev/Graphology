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
        #region Dependencies

        private readonly IGraphTraversal _graphTraversal;
        private readonly IGraphVisualization _graphVisualization;

        #endregion Dependencies

        #region Constructors

        public GraphologistComponents(IGraphTraversal graphTraversal, IGraphVisualization graphVisualization)
        {
            if (graphTraversal == null) throw new ArgumentNullException("graphTraversal");
            if (graphVisualization == null) throw new ArgumentNullException("graphVisualization");

            this._graphTraversal = graphTraversal;
            this._graphVisualization = graphVisualization;
        }

        #endregion Constructors

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
        #region Constructors

        public DefaultGraphologistComponents()
            : base(new DefaultGraphTraversal(), new DefaultGraphVisualization()) { }

        public DefaultGraphologistComponents(TypeExclusions typeExclusions)
            : base(new DefaultGraphTraversal(typeExclusions), new DefaultGraphVisualization()) { }

        #endregion Constructors
    }

    public interface IGraphologist
    {
        string Graph(object targetObject);
    }

    public class Graphologist : IGraphologist
    {
        #region Dependencies

        private readonly IGraphTraversal _traversal;
        private readonly IGraphVisualization _visualization;

        #endregion Dependencies

        #region Constructors

        public Graphologist()
            : this(new DefaultGraphologistComponents()) { }

        public Graphologist(TypeExclusions typeExclusions)
            : this(new DefaultGraphologistComponents(typeExclusions)) { }

        public Graphologist(IGraphologistComponents components)
            : this(components.GraphTraversal, components.GraphVisualization) { }

        public Graphologist(IGraphTraversal traversal, IGraphVisualization visualization)
        {
            if (traversal == null) throw new ArgumentNullException("traversal");
            if (visualization == null) throw new ArgumentNullException("visualization");

            this._traversal = traversal;
            this._visualization = visualization;
        }

        #endregion Constructors

        #region IGraphologist Members

        public string Graph(object targetObject)
        {
            if (targetObject == null) throw new ArgumentNullException("targetObject");

            string graph = this._visualization.Draw(this._traversal.Traverse(targetObject));

            return graph;
        }

        #endregion IGraphologist Members
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
        #region Dependencies

        private readonly IGetObjectGraph _getObjectGraph;

        #endregion Dependencies

        #region Constructors

        public GraphTraversal(IGetObjectGraph getObjectGraph)
        {
            if (getObjectGraph == null) throw new ArgumentNullException("getObjectGraph");

            this._getObjectGraph = getObjectGraph;
        }

        #endregion Constructors

        #region IGraphTraversal Members

        public GraphNode Traverse(object targetObject)
        {
            if (targetObject == null) throw new ArgumentNullException("targetObject");

            GraphNode graph = this._getObjectGraph.For(targetObject, targetObject.GetType(), "root", new List<object>());

            return graph;
        }

        #endregion IGraphTraversal Members
    }

    public class DefaultGraphTraversal : GraphTraversal
    {
        #region Constructors

        public DefaultGraphTraversal()
            : this(DefaultTypeExclusions, DefaultTypeFieldExclusion)
        {
        }

        public DefaultGraphTraversal(TypeExclusions typeExclusions)
            : this(typeExclusions, DefaultTypeFieldExclusion)
        {
        }

        public DefaultGraphTraversal(ITypeFieldExclusion typeFieldExclusion)
            : this(DefaultTypeExclusions, typeFieldExclusion)
        {
        }

        public DefaultGraphTraversal(TypeExclusions typeExclusions, ITypeFieldExclusion typeFieldExclusion)
            : base(BuildGetObjectGraph(typeExclusions, typeFieldExclusion))
        {
        }

        #endregion Constructors

        #region Default exclusions

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

        #endregion Default exclusions

        #region Builder methods

        public static IGetObjectGraph BuildGetObjectGraph(TypeExclusions typeExclusions, ITypeFieldExclusion typeFieldExclusion)
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

        #endregion Builder methods
    }

    public class DefaultGraphVisualization : TextGraphVisualization
    {
        #region Constructors

        public DefaultGraphVisualization()
            : base(BuildGetNodeString()) { }

        #endregion Constructors

        #region Builder methods

        public static IGetNodeString BuildGetNodeString()
        {
            LazyGetTypeNameString lazyGetTypeNameString = new LazyGetTypeNameString();
            IGetTypeNameString getTypeNameString =
                new CompositeGetTypeNameString(new RecursiveGenericTypeGetTypeNameString(lazyGetTypeNameString),
                                               new DefaultGetTypeNameString());
            lazyGetTypeNameString.GetTypeNameString = getTypeNameString;

            IGetNodeString getNodeString = new DefaultGetNodeString(new DefaultGetDepthString(),
                                                                    new DefaultGetMemberTypesString(lazyGetTypeNameString));

            return getNodeString;
        }

        #endregion Builder methods
    }

    #endregion Facade objects

    #region Graph visualization

    public interface IGraphVisualization
    {
        string Draw(GraphNode graphNode);
    }

    public class TextGraphVisualization : IGraphVisualization
    {
        #region Dependencies

        private readonly IGetNodeString _getNodeString;

        #endregion Dependencies

        #region Constructors

        public TextGraphVisualization(IGetNodeString getNodeString)
        {
            if (getNodeString == null) throw new ArgumentNullException("getNodeString");

            this._getNodeString = getNodeString;
        }

        #endregion Constructors

        #region IGraphVisualization Members

        public string Draw(GraphNode graphNode)
        {
            return this.Draw(graphNode, 0);
        }

        #endregion IGraphVisualization Members

        #region Internal methods

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

        #endregion Internal methods
    }

    public interface IGetNodeString
    {
        string For(GraphNode graphNode, int depth);
    }

    public class DefaultGetNodeString : IGetNodeString
    {
        #region Dependencies

        private readonly IGetDepthString _getDepthString;
        private readonly IGetMemberTypesString _getMemberTypesString;

        #endregion Dependencies

        #region Constructors

        public DefaultGetNodeString(IGetDepthString getDepthString, IGetMemberTypesString getMemberTypesString)
        {
            if (getDepthString == null) throw new ArgumentNullException("getDepthString");
            if (getMemberTypesString == null) throw new ArgumentNullException("getMemberTypesString");

            this._getDepthString = getDepthString;
            this._getMemberTypesString = getMemberTypesString;
        }

        #endregion Constructors

        #region IGetNodeString Members

        public string For(GraphNode graphNode, int depth)
        {
            if (graphNode == null) throw new ArgumentNullException("graphNode");

            string depthString = this._getDepthString.For(depth);
            string memberTypesString = this._getMemberTypesString.For(graphNode);

            string nodeString = string.Format(this._format, depthString, memberTypesString);

            if (graphNode.IsRecursionStart)
            {
                nodeString += " (recursed)";
            }

            return nodeString;
        }

        #endregion IGetNodeString Members

        private string _format = "{0}{1}";

        public string Format
        {
            get { return this._format; }
            set { this._format = value; }
        }
    }

    public interface IGetDepthString
    {
        string For(int depth);
    }

    public class DefaultGetDepthString : IGetDepthString
    {
        #region IGetDepthString Members

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

        #endregion IGetDepthString Members
    }

    public interface IGetMemberTypesString
    {
        string For(GraphNode graphNode);
    }

    public class DefaultGetMemberTypesString : IGetMemberTypesString
    {
        #region Dependencies

        private readonly IGetTypeNameString _getTypeNameString;

        #endregion Dependencies

        #region Constructors

        public DefaultGetMemberTypesString(IGetTypeNameString getTypeNameString)
        {
            if (getTypeNameString == null) throw new ArgumentNullException("getTypeNameString");

            this._getTypeNameString = getTypeNameString;
        }

        #endregion Constructors

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
        #region IGetTypeNameString Members

        public bool For(Type type, out string typeName)
        {
            if (type == null) throw new ArgumentNullException("type");

            return this._getTypeNameString.For(type, out typeName);
        }

        #endregion IGetTypeNameString Members

        #region Relayed instance

        private IGetTypeNameString _getTypeNameString;

        public IGetTypeNameString GetTypeNameString
        {
            get { return this._getTypeNameString; }
            set { this._getTypeNameString = value; }
        }

        #endregion Relayed instance
    }

    public class CompositeGetTypeNameString : IGetTypeNameString
    {
        #region Dependencies

        private readonly IEnumerable<IGetTypeNameString> _innerInstances;

        #endregion Dependencies

        #region Configuration

        public IEnumerable<IGetTypeNameString> InnerInstances
        {
            get { return this._innerInstances; }
        }

        #endregion Configuration

        #region Constructors

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

        #endregion Constructors

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
        #region Dependencies

        private readonly IGetTypeNameString _innerGetTypeNameString;

        #endregion Dependencies

        #region Constructors

        public RecursiveGenericTypeGetTypeNameString(IGetTypeNameString innerGetTypeNameString)
        {
            if (innerGetTypeNameString == null) throw new ArgumentNullException("innerGetTypeNameString");

            this._innerGetTypeNameString = innerGetTypeNameString;
        }

        #endregion Constructors

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
        #region Dependencies

        private readonly TypeExclusions _typeExclusions = new TypeExclusions();

        #endregion Dependencies

        #region Constructors

        protected TypeExclusionsClientBase()
        {
        }

        protected TypeExclusionsClientBase(TypeExclusions typeExclusions)
        {
            if (typeExclusions == null) throw new ArgumentNullException("typeExclusions");

            this._typeExclusions.Add(typeExclusions);
        }

        #endregion Constructors

        #region Methods

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

        #endregion Methods
    }

    #region IGetObjectGraph

    public interface IGetObjectGraph
    {
        GraphNode For(object currentObject, Type referenceType, string referenceName, IEnumerable<object> graphPath);
    }

    public class LazyGetObjectGraph : IGetObjectGraph
    {
        #region Relayed instance

        public IGetObjectGraph GetObjectGraph { get; set; }

        #endregion Relayed instance

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
        #region Dependencies

        private readonly IGetSubGraph _getSubGraph;

        #endregion Dependencies

        #region Constructors

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

        #endregion Constructors

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
                                 ObjectHashCode = currentObject.GetHashCode()
                             };

            if (graphPath.Contains(currentObject))
            {
                node.IsRecursionStart = true;
            }
            else if (!DoNotFollowType(currentObject.GetType()))
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
        #region Dependencies

        private readonly IEnumerable<IGetSubGraph> _getSubGraphRepresentations;

        #endregion Dependencies

        #region Constructors

        public CompositeGetSubGraph(params IGetSubGraph[] getSubGraphRepresentations)
        {
            if (getSubGraphRepresentations == null) throw new ArgumentNullException("getSubGraphRepresentations");

            this._getSubGraphRepresentations = getSubGraphRepresentations;
        }

        public CompositeGetSubGraph(IEnumerable<IGetSubGraph> getSubGraphRepresentations)
        {
            if (getSubGraphRepresentations == null) throw new ArgumentNullException("getSubGraphRepresentations");

            this._getSubGraphRepresentations = getSubGraphRepresentations;
        }

        #endregion Constructors

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
        #region Dependencies

        private readonly IGetObjectGraph _getObjectGraph;

        #endregion Dependencies

        #region Constructors

        public EnumerableGetSubGraph(IGetObjectGraph getObjectGraph)
        {
            if (getObjectGraph == null) throw new ArgumentNullException("getObjectGraph");

            this._getObjectGraph = getObjectGraph;
        }

        public EnumerableGetSubGraph(IGetObjectGraph getObjectGraph, TypeExclusions typeExclusions)
            : base(typeExclusions)
        {
            if (getObjectGraph == null) throw new ArgumentNullException("getObjectGraph");

            this._getObjectGraph = getObjectGraph;
        }

        #endregion Constructors

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
        #region Dependencies

        private readonly IGetObjectGraph _getObjectGraph;
        private readonly IGetObjectFields _getObjectFields;

        #endregion Dependencies

        #region Constructors

        public DefaultGetSubGraph(IGetObjectGraph getObjectGraph, IGetObjectFields getObjectFields, TypeExclusions typeExclusions)
            : base(typeExclusions)
        {
            if (getObjectGraph == null) throw new ArgumentNullException("getObjectGraph");
            if (getObjectFields == null) throw new ArgumentNullException("getObjectFields");

            this._getObjectGraph = getObjectGraph;
            this._getObjectFields = getObjectFields;
        }

        #endregion Constructors

        #region IGetSubGraph Members

        public bool For(object currentObject, IEnumerable<object> graphPath, out IList<GraphNode> subGraph)
        {
            subGraph = new List<GraphNode>();

            var fieldValues = this._getObjectFields.For(currentObject);

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

        #endregion IGetSubGraph Members
    }

    public interface IGetObjectFields
    {
        IEnumerable<ObjectField> For(object currentObject);
    }

    public class DefaultGetObjectFields : IGetObjectFields
    {
        #region Dependencies

        private readonly IGetTypeFields _getTypeFields;

        #endregion Dependencies

        #region Constructors

        public DefaultGetObjectFields(IGetTypeFields getTypeFields)
        {
            if (getTypeFields == null) throw new ArgumentNullException("getTypeFields");

            this._getTypeFields = getTypeFields;
        }

        #endregion Constructors

        #region IGetObjectFields Members

        public IEnumerable<ObjectField> For(object currentObject)
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

        #endregion IGetObjectFields Members
    }

    public interface IGetTypeFields
    {
        IEnumerable<FieldInfo> For(Type type);
    }

    public class FilteringGetTypeFields : IGetTypeFields
    {
        #region Dependencies

        private readonly IGetTypeFields _getTypeFields;
        private readonly ITypeFieldExclusion _typeFieldExclusion;

        #endregion Dependencies

        #region Constructors

        public FilteringGetTypeFields(IGetTypeFields getTypeFields, ITypeFieldExclusion typeFieldExclusion)
        {
            if (getTypeFields == null) throw new ArgumentNullException("getTypeFields");
            if (typeFieldExclusion == null) throw new ArgumentNullException("typeFieldExclusion");

            this._getTypeFields = getTypeFields;
            this._typeFieldExclusion = typeFieldExclusion;
        }

        #endregion Constructors

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
        #region IGetTypeFields Members

        public IEnumerable<FieldInfo> For(Type type)
        {
            List<FieldInfo> fields = new List<FieldInfo>();

            while ((null != type) && (typeof(object) != type))
            {
                FieldInfo[] fieldsHere = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
                fields.AddRange(fieldsHere);

                type = type.BaseType;
            }

            return fields;
        }

        #endregion IGetTypeFields Members
    }

    public interface ITypeFieldExclusion
    {
        bool AppliesTo(Type type, FieldInfo field);
    }

    public class GenericListItemsTypeFieldExclusion : ITypeFieldExclusion
    {
        #region ITypeFieldExclusion Members

        public bool AppliesTo(Type type, FieldInfo field)
        {
            bool typeIsGenericList = ((type.IsGenericType) && (typeof(List<>) == type.GetGenericTypeDefinition()));
            bool fieldIsItems = ("_items" == field.Name);

            bool applies = (typeIsGenericList) && (fieldIsItems);

            return applies;
        }

        #endregion ITypeFieldExclusion Members
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

        public bool IsRecursionStart { get; set; }

        public int ObjectHashCode { get; set; }

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
        #region Dependencies

        private readonly IEnumerable<ITypeExclusion> _exclusions;

        #endregion Dependencies

        #region Constructors

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

        #endregion Constructors

        #region ITypeExclusion Members

        public bool AppliesTo(Type type)
        {
            return this._exclusions.Any(rule => rule.AppliesTo(type));
        }

        #endregion ITypeExclusion Members

        #region Configuration

        public IEnumerable<ITypeExclusion> TypeExclusions
        {
            get { return this._exclusions; }
        }

        #endregion Configuration
    }

    public class FuncTypeExclusion : ITypeExclusion
    {
        #region Dependencies

        private readonly Func<Type, bool> _rule;

        #endregion Dependencies

        #region Constructors

        public FuncTypeExclusion(Func<Type, bool> rule)
        {
            if (rule == null) throw new ArgumentNullException("rule");

            this._rule = rule;
        }

        #endregion Constructors

        #region TypeExclusion Members

        public bool AppliesTo(Type type)
        {
            return this._rule(type);
        }

        #endregion TypeExclusion Members
    }

    public class ExactNamespaceTypeExclusion : FuncTypeExclusion
    {
        #region Dependencies

        private readonly string _ns;

        #endregion Dependencies

        #region Constructors

        public ExactNamespaceTypeExclusion(string @namespace)
            : base(type =>
                   {
                       @namespace = Regex.Replace(@namespace, @"\.$", "");

                       Regex rx = new Regex(string.Format(@"^{0}.[^\.]*$", @namespace));
                       bool applies = rx.IsMatch(type.FullName);

                       return applies;
                   })
        {
            this._ns = @namespace;
        }

        #endregion Constructors

        #region Configuration

        public string Namespace
        {
            get { return this._ns; }
        }

        #endregion Configuration
    }

    public class RootNamespaceTypeExclusion : FuncTypeExclusion
    {
        #region Dependencies

        private readonly string _ns;

        #endregion Dependencies

        #region Constructors

        public RootNamespaceTypeExclusion(string @namespace)
            : base(type =>
                   {
                       @namespace = Regex.Replace(@namespace, @"\.$", "");
                       bool applies = type.FullName.StartsWith(string.Format("{0}.", @namespace));

                       return applies;
                   })
        {
            this._ns = @namespace;
        }

        #endregion Constructors

        #region Configuration

        public string Namespace
        {
            get { return this._ns; }
        }

        #endregion Configuration
    }

    public class ConcreteTypeExclusion : ITypeExclusion
    {
        #region Dependencies

        private readonly IEnumerable<Type> _types;

        #endregion Dependencies

        #region Constructors

        public ConcreteTypeExclusion(params Type[] types)
        {
            if (types == null) throw new ArgumentNullException("types");

            this._types = types;
        }

        public ConcreteTypeExclusion(IEnumerable<Type> types)
        {
            if (types == null) throw new ArgumentNullException("types");

            this._types = types;
        }

        #endregion Constructors

        #region TypeExclusion Members

        public bool AppliesTo(Type type)
        {
            bool applies = this._types.Contains(type);

            return applies;
        }

        #endregion TypeExclusion Members

        #region Configuration

        public IEnumerable<Type> Types
        {
            get { return this._types; }
        }

        #endregion Configuration
    }

    #endregion Exclusion Rules

    public interface IGetRecurringObjectHashes
    {
        IEnumerable<int> From(GraphNode graph);
    }

    public class DefaultGetRecurringObjectHashes : IGetRecurringObjectHashes
    {
        #region IGetRecurringObjectHashes Members

        public IEnumerable<int> From(GraphNode graph)
        {
            IEnumerable<GraphNode> nodes = graph.Traverse().Where(n => !n.IsRecursionStart);
            IEnumerable<GraphNode> recurringNodes = nodes.GroupBy(n => n.ObjectHashCode).Where(g => g.Count() > 1).Select(g => g.First());

            int removed = 0;

            do
            {
                IEnumerable<GraphNode> toRemove = recurringNodes.Where(n => recurringNodes.Any(n2 => n2.SubGraph.Contains(n))).ToList();

                removed = toRemove.Count();

                recurringNodes = recurringNodes.Except(toRemove);
            }
            while (removed > 0);

            IEnumerable<int> hashes = recurringNodes.Select(n => n.ObjectHashCode);

            return hashes;
        }

        #endregion IGetRecurringObjectHashes Members
    }

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