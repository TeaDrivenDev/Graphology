using System;

namespace TeaDriven.Graphology.Visualization
{
    public class DefaultGetNextLetterCode : IGetNextLetterCode
    {
        public string From(string previous)
        {
            char prev = previous[0];

            return Convert.ToChar(Convert.ToInt32(prev) + 1).ToString();
        }
    }
}