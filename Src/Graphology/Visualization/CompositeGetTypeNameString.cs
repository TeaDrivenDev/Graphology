using System;
using System.Collections.Generic;

namespace TeaDriven.Graphology.Visualization
{
    public class CompositeGetTypeNameString : IGetTypeNameString
    {
        #region Dependencies

        private readonly IEnumerable<IGetTypeNameString> _innerInstances;

        #endregion Dependencies

        #region Configuration

        public IEnumerable<IGetTypeNameString> InnerInstances
        {
            get { return this._innerInstances; }
        }

        #endregion Configuration

        #region Constructors

        public CompositeGetTypeNameString(params IGetTypeNameString[] innerInstances)
        {
            if (innerInstances == null) throw new ArgumentNullException("innerInstances");

            this._innerInstances = innerInstances;
        }

        public CompositeGetTypeNameString(IEnumerable<IGetTypeNameString> innerInstances)
        {
            if (innerInstances == null) throw new ArgumentNullException("innerInstances");

            this._innerInstances = innerInstances;
        }

        #endregion Constructors

        #region IGetTypeNameString Members

        public bool For(Type type, out string typeName)
        {
            if (type == null) throw new ArgumentNullException("type");

            typeName = "";

            bool handled = false;

            foreach (IGetTypeNameString innerInstance in this._innerInstances)
            {
                handled = innerInstance.For(type, out typeName);

                if (handled)
                {
                    break;
                }
            }

            return handled;
        }

        #endregion IGetTypeNameString Members
    }
}