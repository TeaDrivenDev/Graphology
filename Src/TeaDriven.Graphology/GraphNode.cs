using System;
using System.Collections.Generic;

namespace TeaDriven.Graphology
{
    public class GraphNode
    {
        public Type ReferenceType { get; set; }

        public Type ObjectType { get; set; }

        public string ReferenceName { get; set; }

        private IEnumerable<GraphNode> _subGraph = new List<GraphNode>();

        public IEnumerable<GraphNode> SubGraph
        {
            get { return this._subGraph; }
            set { this._subGraph = value; }
        }

        public bool IsRecursionStart { get; set; }

        public int ObjectHashCode { get; set; }

        public override string ToString()
        {
            // This is only to see stuff more easily in the debug window; it is too unflexible for practical use
            string text = string.Format("{0} : {1}", this.ObjectType.Name, this.ReferenceType.Name);

            return text;
        }
    }
}