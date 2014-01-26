using System;
using System.Collections.Generic;
using System.Linq;

namespace TeaDriven.Graphology.Traversal
{
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
}