namespace TeaDriven.Graphology.Visualization
{
    public class DefaultGraphVisualization : RecurrenceMarkingTextGraphVisualization
    {
        #region Constructors

        public DefaultGraphVisualization()
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
}