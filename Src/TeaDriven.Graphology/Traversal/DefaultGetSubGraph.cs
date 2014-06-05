using System;
using System.Collections.Generic;
using System.Linq;

namespace TeaDriven.Graphology.Traversal
{
    public class DefaultGetSubGraph : TypeExclusionsClientBase, IGetSubGraph
    {
        #region Dependencies

        private readonly IGetObjectGraph _getObjectGraph;
        private readonly IGetObjectFields _getObjectFields;

        #endregion Dependencies

        #region Constructors

        public DefaultGetSubGraph(IGetObjectGraph getObjectGraph, IGetObjectFields getObjectFields, TypeExclusions typeExclusions)
            : base(typeExclusions)
        {
            if (getObjectGraph == null) throw new ArgumentNullException("getObjectGraph");
            if (getObjectFields == null) throw new ArgumentNullException("getObjectFields");

            this._getObjectGraph = getObjectGraph;
            this._getObjectFields = getObjectFields;
        }

        #endregion Constructors

        #region IGetSubGraph Members

        public bool For(object currentObject, IEnumerable<object> graphPath, out IList<GraphNode> subGraph)
        {
            subGraph = new List<GraphNode>();

            var fieldValues = this._getObjectFields.For(currentObject);

            foreach (var fieldValue in fieldValues.Where(v => null != v.FieldValue))
            {
                if ((!this.TypeIsExcluded(fieldValue.ReferenceType)) && (!this.TypeIsExcluded(fieldValue.FieldValue.GetType())))
                {
                    subGraph.Add(this._getObjectGraph.For(fieldValue.FieldValue, fieldValue.ReferenceType, fieldValue.ReferenceName,
                                                          graphPath));
                }
            }

            return true;
        }

        #endregion IGetSubGraph Members
    }
}