using System;
using System.Collections.Generic;
using System.Linq;

namespace TeaDriven.Graphology
{
    public class DefaultGraphVisualization2 : RecurrenceMarkingTextGraphVisualization
    {
        #region Constructors

        public DefaultGraphVisualization2()
            : base(
                BuildGetNodeString(), new DefaultGetHashCodeMappings(new DefaultGetNextLetterCode()), new DefaultGetRecurringObjectHashes())
        {
        }

        #endregion Constructors

        #region Builder methods

        public static IGetNodeString BuildGetNodeString()
        {
            LazyGetTypeNameString lazyGetTypeNameString = new LazyGetTypeNameString();
            IGetTypeNameString getTypeNameString =
                new CompositeGetTypeNameString(new RecursiveGenericTypeGetTypeNameString(lazyGetTypeNameString),
                                               new DefaultGetTypeNameString());
            lazyGetTypeNameString.GetTypeNameString = getTypeNameString;

            IGetNodeString getNodeString = new DefaultGetNodeString(new DefaultGetDepthString(),
                                                                    new DefaultGetMemberTypesString(lazyGetTypeNameString))
                                           {
                                               Format = "{0}{{0}}{1}"
                                           };

            return getNodeString;
        }

        #endregion Builder methods
    }

    public class RecurrenceMarkingTextGraphVisualization : IGraphVisualization
    {
        #region Dependencies

        private readonly IGetNodeString _getNodeString;
        private readonly IGetHashCodeMappings _getHashCodeMappings;
        private readonly IGetRecurringObjectHashes _getRecurringObjectHashes;

        #endregion Dependencies

        #region Constructors

        public RecurrenceMarkingTextGraphVisualization(IGetNodeString getNodeString,
                                                       IGetHashCodeMappings getHashCodeMappings,
                                                       IGetRecurringObjectHashes getRecurringObjectHashes)
        {
            if (getNodeString == null) throw new ArgumentNullException("getNodeString");
            if (getHashCodeMappings == null) throw new ArgumentNullException("getHashCodeMappings");
            if (getRecurringObjectHashes == null) throw new ArgumentNullException("getRecurringObjectHashes");

            this._getNodeString = getNodeString;
            _getHashCodeMappings = getHashCodeMappings;
            _getRecurringObjectHashes = getRecurringObjectHashes;
        }

        #endregion Constructors

        #region IGraphVisualization Members

        public string Draw(GraphNode graphNode)
        {
            IEnumerable<int> recurringObjectHashes = this._getRecurringObjectHashes.From(graphNode);
            IDictionary<int, string> hashCodeMappings = this._getHashCodeMappings.For(recurringObjectHashes);

            return this.Draw(graphNode, 0, hashCodeMappings);
        }

        #endregion IGraphVisualization Members

        #region Internal methods

        private string Draw(GraphNode graphNode, int depth, IDictionary<int, string> hashCodeMappings)
        {
            string hashSuffix = (hashCodeMappings.ContainsKey(graphNode.ObjectHashCode)
                                     ? string.Format("({0}) ", hashCodeMappings[graphNode.ObjectHashCode])
                                     : "");

            string graph = string.Format(this._getNodeString.For(graphNode, depth), hashSuffix) + Environment.NewLine;

            depth++;

            foreach (GraphNode subGraphNode in graphNode.SubGraph)
            {
                graph += this.Draw(subGraphNode, depth, hashCodeMappings);
            }

            return graph;
        }

        #endregion Internal methods
    }

    public interface IGetHashCodeMappings
    {
        IDictionary<int, string> For(IEnumerable<int> hashCodes);
    }

    public class DefaultGetHashCodeMappings : IGetHashCodeMappings
    {
        private readonly IGetNextLetterCode _getNextLetterCode;

        public DefaultGetHashCodeMappings(IGetNextLetterCode getNextLetterCode)
        {
            if (getNextLetterCode == null) throw new ArgumentNullException("getNextLetterCode");

            this._getNextLetterCode = getNextLetterCode;
        }

        public IDictionary<int, string> For(IEnumerable<int> hashCodes)
        {
            string code = "A";

            return hashCodes.ToDictionary(h => h, h =>
                                                  {
                                                      string x = code;
                                                      code = this._getNextLetterCode.From(x);
                                                      return x;
                                                  });
        }
    }

    public interface IGetNextLetterCode
    {
        string From(string previous);
    }

    public class DefaultGetNextLetterCode : IGetNextLetterCode
    {
        public string From(string previous)
        {
            char prev = previous[0];

            return Convert.ToChar(Convert.ToInt32(prev) + 1).ToString();
        }
    }
}