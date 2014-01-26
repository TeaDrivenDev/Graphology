using System;

namespace TeaDriven.Graphology.Traversal
{
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
}