using System;

namespace TeaDriven.Graphology.Visualization
{
    public interface IGetTypeNameString
    {
        bool For(Type type, out string typeName);
    }
}