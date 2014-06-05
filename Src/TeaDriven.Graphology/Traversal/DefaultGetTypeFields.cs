using System;
using System.Collections.Generic;
using System.Reflection;

namespace TeaDriven.Graphology.Traversal
{
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
}