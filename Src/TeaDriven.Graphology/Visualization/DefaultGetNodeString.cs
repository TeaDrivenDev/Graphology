using System;

namespace TeaDriven.Graphology.Visualization
{
    public class DefaultGetNodeString : IGetNodeString
    {
        #region Dependencies

        private readonly IGetDepthString _getDepthString;
        private readonly IGetMemberTypesString _getMemberTypesString;

        #endregion Dependencies

        #region Constructors

        public DefaultGetNodeString(IGetDepthString getDepthString, IGetMemberTypesString getMemberTypesString)
        {
            if (getDepthString == null) throw new ArgumentNullException("getDepthString");
            if (getMemberTypesString == null) throw new ArgumentNullException("getMemberTypesString");

            this._getDepthString = getDepthString;
            this._getMemberTypesString = getMemberTypesString;
        }

        #endregion Constructors

        #region IGetNodeString Members

        public string For(GraphNode graphNode, int depth)
        {
            if (graphNode == null) throw new ArgumentNullException("graphNode");

            string depthString = this._getDepthString.For(depth);
            string memberTypesString = this._getMemberTypesString.For(graphNode);

            string nodeString = string.Format(this._format, depthString, memberTypesString);

            if (graphNode.IsRecursionStart)
            {
                nodeString += " (recursed)";
            }

            return nodeString;
        }

        #endregion IGetNodeString Members

        private string _format = "{0}{1}";

        public string Format
        {
            get { return this._format; }
            set { this._format = value; }
        }
    }
}