using System;

namespace TeaDriven.Graphology.Visualization
{
    public class DefaultGetMemberTypesString : IGetMemberTypesString
    {
        #region Dependencies

        private readonly IGetTypeNameString _getTypeNameString;

        #endregion Dependencies

        #region Constructors

        public DefaultGetMemberTypesString(IGetTypeNameString getTypeNameString)
        {
            if (getTypeNameString == null) throw new ArgumentNullException("getTypeNameString");

            this._getTypeNameString = getTypeNameString;
        }

        #endregion Constructors

        #region IGetMemberTypesString Members

        public string For(GraphNode graphNode)
        {
            if (graphNode == null) throw new ArgumentNullException("graphNode");

            string objectTypeName;
            this._getTypeNameString.For(graphNode.ObjectType, out objectTypeName);
            string referenceTypeName;
            this._getTypeNameString.For(graphNode.ReferenceType, out referenceTypeName);

            string memberTypesString = string.Format("{0} : {1}", objectTypeName, referenceTypeName);

            return memberTypesString;
        }

        #endregion IGetMemberTypesString Members
    }
}