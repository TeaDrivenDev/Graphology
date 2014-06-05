using System.Collections.Generic;

namespace TeaDriven.Graphology.Traversal
{
    public interface IGetObjectFields
    {
        IEnumerable<ObjectField> For(object currentObject);
    }
}