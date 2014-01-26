namespace TeaDriven.Graphology.Visualization
{
    public class DefaultGetDepthString : IGetDepthString
    {
        #region IGetDepthString Members

        public string For(int depth)
        {
            var depthString = "";
            if (depth > 0)
            {
                depthString += new string(' ', 3 * (depth - 1));
                depthString += " > ";
            }
            return depthString;
        }

        #endregion IGetDepthString Members
    }
}