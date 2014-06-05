using System;

namespace TeaDriven.Graphology.Visualization
{
    public class DefaultGetTypeNameString : IGetTypeNameString
    {
        #region IGetTypeNameString Members

        public bool For(Type type, out string typeName)
        {
            if (type == null) throw new ArgumentNullException("type");

            typeName = type.Name;

            return true;
        }

        #endregion IGetTypeNameString Members
    }
}