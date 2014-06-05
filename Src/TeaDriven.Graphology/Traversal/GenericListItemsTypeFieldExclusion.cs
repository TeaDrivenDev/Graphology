using System;
using System.Collections.Generic;
using System.Reflection;

namespace TeaDriven.Graphology.Traversal
{
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
}