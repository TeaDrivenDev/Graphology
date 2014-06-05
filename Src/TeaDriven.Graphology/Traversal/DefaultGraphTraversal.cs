using System.Collections.Generic;

namespace TeaDriven.Graphology.Traversal
{
    public class DefaultGraphTraversal : GraphTraversal
    {
        #region Constructors

        public DefaultGraphTraversal()
            : this(DefaultTypeExclusions, DefaultTypeFieldExclusion)
        {
        }

        public DefaultGraphTraversal(TypeExclusions typeExclusions)
            : this(typeExclusions, DefaultTypeFieldExclusion)
        {
        }

        public DefaultGraphTraversal(ITypeFieldExclusion typeFieldExclusion)
            : this(DefaultTypeExclusions, typeFieldExclusion)
        {
        }

        public DefaultGraphTraversal(TypeExclusions typeExclusions, ITypeFieldExclusion typeFieldExclusion)
            : base(BuildGetObjectGraph(typeExclusions, typeFieldExclusion))
        {
        }

        #endregion Constructors

        #region Default exclusions

        private static readonly TypeExclusions _defaultTypeExclusions = new MinimalTypeExclusions();
        private static readonly ITypeFieldExclusion _defaultTypeFieldExclusion = new GenericListItemsTypeFieldExclusion();

        public static TypeExclusions DefaultTypeExclusions
        {
            get { return _defaultTypeExclusions; }
        }

        public static ITypeFieldExclusion DefaultTypeFieldExclusion
        {
            get { return _defaultTypeFieldExclusion; }
        }

        #endregion Default exclusions

        #region Builder methods

        public static IGetObjectGraph BuildGetObjectGraph(TypeExclusions typeExclusions, ITypeFieldExclusion typeFieldExclusion)
        {
            LazyGetObjectGraph getObjectGraph = new LazyGetObjectGraph();

            IGetSubGraph getSubGraph = new CompositeGetSubGraph(new List<IGetSubGraph>()
                                                                {
                                                                    new EnumerableGetSubGraph(getObjectGraph, typeExclusions),
                                                                    new DefaultGetSubGraph(getObjectGraph,
                                                                                           new DefaultGetObjectFields(
                                                                                               new FilteringGetTypeFields(
                                                                                                   new DefaultGetTypeFields(),
                                                                                                   typeFieldExclusion)),
                                                                                           typeExclusions)
                                                                });

            getObjectGraph.GetObjectGraph = new DefaultGetObjectGraph(getSubGraph, typeExclusions);

            return getObjectGraph;
        }

        #endregion Builder methods
    }
}