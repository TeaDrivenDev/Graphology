﻿using Fasterflect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TeaDriven.Graphology.Traversal
{
    public class DefaultGetObjectFields : IGetObjectFields
    {
        #region Dependencies

        private readonly IGetTypeFields _getTypeFields;

        #endregion Dependencies

        #region Constructors

        public DefaultGetObjectFields(IGetTypeFields getTypeFields)
        {
            if (getTypeFields == null) throw new ArgumentNullException("getTypeFields");

            this._getTypeFields = getTypeFields;
        }

        #endregion Constructors

        #region IGetObjectFields Members

        public IEnumerable<ObjectField> For(object currentObject)
        {
            Type type = currentObject.GetType();

            IEnumerable<FieldInfo> fields = this._getTypeFields.For(type);

            IEnumerable<ObjectField> fieldValues = fields.Select(fi => new ObjectField()
                                                                       {
                                                                           ReferenceType = fi.FieldType,
                                                                           ReferenceName = fi.Name,
                                                                           FieldValue = currentObject.GetFieldValue(fi.Name)
                                                                       });

            return fieldValues;
        }

        #endregion IGetObjectFields Members
    }
}