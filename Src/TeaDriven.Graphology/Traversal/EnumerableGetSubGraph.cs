using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TeaDriven.Graphology.Traversal
{
    public class EnumerableGetSubGraph : TypeExclusionsClientBase, IGetSubGraph
    {
        #region Dependencies

        private readonly IGetObjectGraph _getObjectGraph;

        #endregion Dependencies

        #region Constructors

        public EnumerableGetSubGraph(IGetObjectGraph getObjectGraph)
        {
            if (getObjectGraph == null) throw new ArgumentNullException("getObjectGraph");

            this._getObjectGraph = getObjectGraph;
        }

        public EnumerableGetSubGraph(IGetObjectGraph getObjectGraph, TypeExclusions typeExclusions)
            : base(typeExclusions)
        {
            if (getObjectGraph == null) throw new ArgumentNullException("getObjectGraph");

            this._getObjectGraph = getObjectGraph;
        }

        #endregion Constructors

        #region IGetSubGraph Members

        public bool For(object currentObject, IEnumerable<object> graphPath, out IList<GraphNode> subGraph)
        {
            if (currentObject == null) throw new ArgumentNullException("currentObject");
            if (graphPath == null) throw new ArgumentNullException("graphPath");

            subGraph = new List<GraphNode>();

            bool handled = false;

            var type = currentObject.GetType();

            var genericIenumerable =
                type.GetInterfaces().FirstOrDefault(i => (i.IsGenericType) && (i.GetGenericTypeDefinition() == typeof(IEnumerable<>)));

            if (null != genericIenumerable)
            {
                Type itemReferenceType = genericIenumerable.GetGenericArguments().First();

                foreach (var item in currentObject as IEnumerable)
                {
                    if (null != item)
                    {
                        Type itemConcreteType = item.GetType();

                        if ((!this.TypeIsExcluded(itemReferenceType)) && (!this.TypeIsExcluded(itemConcreteType)))
                        {
                            subGraph.Add(this._getObjectGraph.For(item, itemReferenceType, "Item", graphPath));
                        }
                    }
                }

                handled = true;
            }

            return handled;
        }

        #endregion IGetSubGraph Members
    }
}