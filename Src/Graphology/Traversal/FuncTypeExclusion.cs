using System;

namespace TeaDriven.Graphology.Traversal
{
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
}