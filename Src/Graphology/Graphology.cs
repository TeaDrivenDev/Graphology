using Fasterflect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TeaDriven.Graphology
{
    public class CreateGraphologist
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

        public Graphologist Now()
        {
            LazyGetObjectGraph getObjectGraph = new LazyGetObjectGraph();

            IGetSubGraph getSubGraph = new CompositeGetSubGraph(new List<IGetSubGraph>()
                                                                {
                                                                    new EnumerableGetSubGraph(getObjectGraph),
                                                                    new DefaultGetSubGraph(getObjectGraph)
                                                                });

            getObjectGraph.GetObjectGraph = new GetObjectGraph(getSubGraph, this.ExclusionRules)
                                             {
                                                 ShowDependencyTypes = this._showDependencyTypes
                                             };

            Graphologist graphologist = new Graphologist(getObjectGraph);

            return graphologist;
        }
    }

    #region IGetObjectGraph

    public interface IGetObjectGraph
    {
        bool ShowDependencyTypes { get; set; }

        string For(object dings, string referenceTypeName, int depth, IEnumerable<object> graphPath);
    }

    public class LazyGetObjectGraph : IGetObjectGraph
    {
        #region IGetObjectGraph Members

        public bool ShowDependencyTypes
        {
            get { return this.GetObjectGraph.ShowDependencyTypes; }
            set { this.GetObjectGraph.ShowDependencyTypes = value; }
        }

        public string For(object dings, string referenceTypeName, int depth, IEnumerable<object> graphPath)
        {
            return this.GetObjectGraph.For(dings, referenceTypeName, depth, graphPath);
        }

        #endregion IGetObjectGraph Members

        public IGetObjectGraph GetObjectGraph { get; set; }
    }

    public class GetObjectGraph : IGetObjectGraph
    {
        public bool ShowDependencyTypes
        {
            get { return this._showDependencyTypes; }
            set { this._showDependencyTypes = value; }
        }

        private readonly ExclusionRulesSet _exclusionRules = new MinimalExclusionRulesSet();
        private readonly IGetSubGraph _getSubGraph;
        private bool _showDependencyTypes = true;

        private ExclusionRulesSet ExclusionRules
        {
            get { return this._exclusionRules; }

            set
            {
                this._exclusionRules.DoNotFollow = value.DoNotFollow;
                this._exclusionRules.Exclude = value.Exclude;
            }
        }

        public GetObjectGraph(IGetSubGraph getSubGraph)
        {
            this._getSubGraph = getSubGraph;
        }

        public GetObjectGraph(IGetSubGraph getSubGraph, ExclusionRulesSet exclusionRules)
            : this(getSubGraph)
        {
            this.ExclusionRules = exclusionRules;
        }

        public string For(object dings, string referenceTypeName, int depth, IEnumerable<object> graphPath)
        {
            var graph = "";

            var dingsType = dings.GetType();

            if (!this.TypeIsExcluded(dingsType))
            {
                graph += this.GetTypeString(referenceTypeName, depth, dingsType);

                if ((!this.DoNotFollowType(dingsType)) && (!graphPath.Contains(dings)))
                {
                    string subGraph;
                    bool handled = this._getSubGraph.For(dings, dingsType, depth, graphPath.Concat(new List<object>() { dings }), out subGraph);
                    graph += subGraph;
                }
            }

            return graph;
        }

        private string GetTypeString(string referenceTypeName, int depth, Type dingsType)
        {
            var depthString = "";
            if (depth > 0)
            {
                depthString += new string(' ', 3 * (depth - 1));
                depthString += " >";
            }

            var memberTypeString = this.GetMemberTypeString(referenceTypeName);
            var graph = string.Format("{0} {1}{2}{3}", depthString, dingsType.Name, memberTypeString, Environment.NewLine);
            return graph;
        }

        private string GetMemberTypeString(string referenceTypeName)
        {
            var memberTypeString = (this.ShowDependencyTypes
                                        ? (string.IsNullOrEmpty(referenceTypeName) ? "" : " : " + referenceTypeName)
                                        : "");

            return memberTypeString;
        }

        private bool TypeIsExcluded(Type t)
        {
            return this._exclusionRules.Exclude.AppliesTo(t);
        }

        private bool DoNotFollowType(Type t)
        {
            return this._exclusionRules.DoNotFollow.AppliesTo(t);
        }
    }

    #endregion IGetObjectGraph

    #region IGetSubGraph

    public interface IGetSubGraph
    {
        bool For(object dings, Type dingsType, int depth, IEnumerable<object> graphPath, out string graph);
    }

    public class CompositeGetSubGraph : IGetSubGraph
    {
        private readonly IEnumerable<IGetSubGraph> _getSubGraphs;

        public CompositeGetSubGraph(IEnumerable<IGetSubGraph> getSubGraphs)
        {
            if (getSubGraphs == null) throw new ArgumentNullException("getSubGraphs");

            this._getSubGraphs = getSubGraphs;
        }

        public bool For(object dings, Type dingsType, int depth, IEnumerable<object> graphPath, out string graph)
        {
            graph = "";

            bool handled = false;

            foreach (IGetSubGraph getSubGraph in this._getSubGraphs)
            {
                handled = getSubGraph.For(dings, dingsType, depth, graphPath, out graph);

                if (handled)
                {
                    // TODO: Unbreak
                    break;
                }
            }

            return handled;
        }
    }

    public class EnumerableGetSubGraph : IGetSubGraph
    {
        private readonly IGetObjectGraph _getObjectGraph;

        public EnumerableGetSubGraph(IGetObjectGraph getObjectGraph)
        {
            _getObjectGraph = getObjectGraph;
        }

        #region IGetSubGraph Members

        public bool For(object dings, Type dingsType, int depth, IEnumerable<object> graphPath, out string graph)
        {
            graph = "";

            bool handled = false;

            var ien = dingsType.Implements<IEnumerable>();

            if (ien)
            {
                var ienu = dings as IEnumerable;

                foreach (var lol in ienu)
                {
                    graph += this._getObjectGraph.For(lol, "", depth + 1, graphPath);
                }

                handled = true;
            }

            return handled;
        }

        #endregion IGetSubGraph Members
    }

    public class DefaultGetSubGraph : IGetSubGraph
    {
        private readonly IGetObjectGraph _getObjectGraph;

        public DefaultGetSubGraph(IGetObjectGraph getObjectGraph)
        {
            _getObjectGraph = getObjectGraph;
        }

        #region IGetSubGraph Members

        public bool For(object dings, Type dingsType, int depth, IEnumerable<object> graphPath, out string graph)
        {
            graph = "";

            var fields = dingsType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

            var fieldValues = fields.Select(fi => new { TypeName = fi.FieldType.Name, FieldValue = dings.GetFieldValue(fi.Name) });

            foreach (var fieldValue in fieldValues.Where(v => null != v.FieldValue))
            {
                graph += this._getObjectGraph.For(fieldValue.FieldValue, fieldValue.TypeName, depth + 1, graphPath);
            }

            return true;
        }

        #endregion IGetSubGraph Members
    }

    #endregion IGetSubGraph

    public class Graphologist
    {
        private readonly IGetObjectGraph _getObjectGraph;

        /// <summary>
        /// Creates a new instance of the Graphologist class
        /// </summary>
        public Graphologist(IGetObjectGraph getObjectGraph)
        {
            _getObjectGraph = getObjectGraph;
        }

        public string Graph(object dings)
        {
            return this._getObjectGraph.For(dings, "", 0, new List<object>());
        }
    }

    #region Exclusion Rules

    public class ExclusionRulesSet
    {
        protected IExclusionRule _excludeFixed = new EmptyExclusionRule();
        private IExclusionRule _exclude = new EmptyExclusionRule();

        public IExclusionRule Exclude
        {
            get { return this._exclude; }
            set
            {
                if (value == null) throw new ArgumentNullException("Exclusion");

                this._exclude = new CompositeExclusionRule(this._excludeFixed, value);
            }
        }

        protected IExclusionRule _doNotFollowFixed = new EmptyExclusionRule();
        private IExclusionRule _doNotFollow = new EmptyExclusionRule();

        public IExclusionRule DoNotFollow
        {
            get { return this._doNotFollow; }
            set
            {
                if (value == null) throw new ArgumentNullException("DoNotFollow");

                this._doNotFollow = new CompositeExclusionRule(this._doNotFollowFixed, value);
            }
        }
    }

    public class MinimalExclusionRulesSet : ExclusionRulesSet
    {
        public MinimalExclusionRulesSet()
        {
            this._doNotFollowFixed = new ExactNamespaceExclusionRule("System");
        }
    }

    public class TestExclusionRulesSet : ExclusionRulesSet
    {
        public TestExclusionRulesSet()
        {
            this._doNotFollowFixed = new RootNamespaceExclusionRule("Castle.Proxies");
        }
    }

    public interface IExclusionRule
    {
        bool AppliesTo(Type type);
    }

    public class EmptyExclusionRule : IExclusionRule
    {
        #region IExclusionRule Members

        public bool AppliesTo(Type type)
        {
            return false;
        }

        #endregion IExclusionRule Members
    }

    public class CompositeExclusionRule : IExclusionRule
    {
        private readonly IEnumerable<IExclusionRule> _exclusionRules;

        public CompositeExclusionRule(IEnumerable<IExclusionRule> exclusionRules)
        {
            if (exclusionRules == null) throw new ArgumentNullException("exclusionRules");

            _exclusionRules = exclusionRules;
        }

        public CompositeExclusionRule(params IExclusionRule[] exclusionRules)
        {
            if (exclusionRules == null) throw new ArgumentNullException("exclusionRules");

            _exclusionRules = exclusionRules;
        }

        #region IExclusionRule Members

        public bool AppliesTo(Type type)
        {
            return this._exclusionRules.Any(rule => rule.AppliesTo(type));
        }

        #endregion IExclusionRule Members
    }

    public class FuncExclusionRule : IExclusionRule
    {
        private readonly Func<Type, bool> _rule;

        public FuncExclusionRule(Func<Type, bool> rule)
        {
            if (rule == null) throw new ArgumentNullException("rule");

            _rule = rule;
        }

        #region IExclusionRule Members

        public bool AppliesTo(Type type)
        {
            return this._rule(type);
        }

        #endregion IExclusionRule Members
    }

    public class ExactNamespaceExclusionRule : FuncExclusionRule
    {
        public ExactNamespaceExclusionRule(string @namespace)
            : base(type =>
                   {
                       @namespace = Regex.Replace(@namespace, @"\.$", "");

                       Regex rx = new Regex(string.Format(@"^{0}.[^\.]*", @namespace));

                       return rx.IsMatch(type.FullName);
                   }) { }
    }

    public class RootNamespaceExclusionRule : FuncExclusionRule
    {
        public RootNamespaceExclusionRule(string @namespace)
            : base(type =>
                   {
                       @namespace = Regex.Replace(@namespace, @"\.$", "");

                       return type.FullName.StartsWith(string.Format("{0}.", @namespace));
                   }) { }
    }

    public class ConcreteTypesExclusionRule : IExclusionRule
    {
        private readonly IEnumerable<Type> _types;

        public ConcreteTypesExclusionRule(params Type[] types)
        {
            if (types == null) throw new ArgumentNullException("types");

            _types = types;
        }

        public ConcreteTypesExclusionRule(IEnumerable<Type> types)
        {
            if (types == null) throw new ArgumentNullException("types");

            _types = types;
        }

        #region IExclusionRule Members

        public bool AppliesTo(Type type)
        {
            return this._types.Contains(type);
        }

        #endregion IExclusionRule Members
    }

    #endregion Exclusion Rules
}