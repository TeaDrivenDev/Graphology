using System;

namespace TeaDriven.Graphology.Traversal
{
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
}