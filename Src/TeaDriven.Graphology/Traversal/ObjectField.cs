using System;

namespace TeaDriven.Graphology.Traversal
{
    public class ObjectField
    {
        public Type ReferenceType { get; set; }

        public string ReferenceName { get; set; }

        public object FieldValue { get; set; }
    }
}