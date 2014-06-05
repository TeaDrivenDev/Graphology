using System.Collections.Generic;

namespace TeaDriven.Graphology.Visualization
{
    public interface IGetHashCodeMappings
    {
        IDictionary<int, string> For(IEnumerable<int> hashCodes);
    }
}