using System;
using System.Collections.Generic;
using System.Linq;

namespace TeaDriven.Graphology.Traversal
{
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
}