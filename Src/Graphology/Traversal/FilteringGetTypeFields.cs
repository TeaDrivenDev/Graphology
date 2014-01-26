using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TeaDriven.Graphology.Traversal
{
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
}