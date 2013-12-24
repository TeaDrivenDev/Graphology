using Fasterflect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TeaDriven.Graphology
{
    public class Graphologist
    {
        private bool _showDependencyTypes = true;

        public bool ShowDependencyTypes
        {
            get { return this._showDependencyTypes; }
            set { this._showDependencyTypes = value; }
        }

        private readonly ExclusionRulesSet _exclusionRulesSet = new MinimalExclusionRulesSet();

        /// <summary>
        /// Creates a new instance of the Graphologist class
        /// </summary>
        public Graphologist()
        {
        }

        /// <summary>
        /// Creates a new instance of the Graphologist class with the specified set of exclusion rules
        /// </summary>
        /// <param name="exclusionRulesSet">A set of rules specifying which types not to recurse into or ignore completely</param>
        public Graphologist(ExclusionRulesSet exclusionRulesSet)
        {
            if (exclusionRulesSet == null) throw new ArgumentNullException("exclusionRulesSet");

            this._exclusionRulesSet.Exclude = exclusionRulesSet.Exclude;
            this._exclusionRulesSet.DoNotFollow = exclusionRulesSet.DoNotFollow;
        }

        public string Graph(object dings)
        {
            return this.GetObjectGraph(dings, "", 0);
        }

        private string GetObjectGraph(object dings, string referenceTypeName, int depth)
        {
            var graph = "";

            var dingsType = dings.GetType();

            if (!this.TypeIsExcluded(dingsType))
            {
                graph += this.GetTypeString(referenceTypeName, depth, dingsType);

                if (!this.DoNotFollowType(dingsType))
                {
                    graph += this.GetSubGraph(dings, dingsType, depth);
                }
            }

            return graph;
        }

        private string GetSubGraph(object dings, Type dingsType, int depth)
        {
            string graph = "";

            var fields = dingsType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

            var fieldValues = fields.Select(fi => new { TypeName = fi.FieldType.Name, FieldValue = dings.GetFieldValue(fi.Name) });

            foreach (var fieldValue in fieldValues.Where(v => null != v.FieldValue))
            {
                graph += this.GetObjectGraph(fieldValue.FieldValue, fieldValue.TypeName, depth + 1);
            }

            return graph;
        }

        private string GetTypeString(string referenceTypeName, int depth, Type dingsType)
        {
            var depthString = "";
            if (depth > 0)
            {
                depthString += new string(' ', 3 * (depth - 1));
                depthString += " ►";
            }

            var memberTypeString = this.GetMemberTypeString(referenceTypeName);
            var graph = string.Format("{0} {1}{2}{3}", depthString, dingsType.Name, memberTypeString, Environment.NewLine);
            return graph;
        }

        private string GetMemberTypeString(string referenceTypeName)
        {
            var memberTypeString = (this.ShowDependencyTypes
                                        ? (string.IsNullOrWhiteSpace(referenceTypeName) ? "" : " : " + referenceTypeName)
                                        : "");

            return memberTypeString;
        }

        private bool TypeIsExcluded(Type t)
        {
            return this._exclusionRulesSet.Exclude.AppliesTo(t);
        }

        private bool DoNotFollowType(Type t)
        {
            return this._exclusionRulesSet.DoNotFollow.AppliesTo(t);
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