using System;

namespace TeaDriven.Graphology.Visualization
{
    public class LazyGetTypeNameString : IGetTypeNameString
    {
        #region IGetTypeNameString Members

        public bool For(Type type, out string typeName)
        {
            if (type == null) throw new ArgumentNullException("type");

            return this._getTypeNameString.For(type, out typeName);
        }

        #endregion IGetTypeNameString Members

        #region Relayed instance

        private IGetTypeNameString _getTypeNameString;

        public IGetTypeNameString GetTypeNameString
        {
            get { return this._getTypeNameString; }
            set { this._getTypeNameString = value; }
        }

        #endregion Relayed instance
    }
}