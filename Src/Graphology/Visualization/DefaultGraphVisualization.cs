namespace TeaDriven.Graphology.Visualization
{
    public class DefaultGraphVisualization : TextGraphVisualization
    {
        #region Constructors

        public DefaultGraphVisualization()
            : base(BuildGetNodeString()) { }

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
                                                                    new DefaultGetMemberTypesString(lazyGetTypeNameString));

            return getNodeString;
        }

        #endregion Builder methods
    }
}