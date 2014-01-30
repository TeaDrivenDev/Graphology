using System;

namespace TeaDriven.Graphology.Visualization
{
    public class DefaultGetNextLetterCode : IGetNextLetterCode
    {
        public string From(string previous)
        {
            if (previous == null) throw new ArgumentNullException("previous");

            char last = previous[previous.Length - 1];
            if ('Z' == last)
            {
                if (1 == previous.Length)
                {
                    return "AA";
                }
                else
                {
                    return this.From(previous.Substring(0, previous.Length - 1)) + 'A';
                }
            }
            else
            {
                last++;
                previous = previous.Substring(0, previous.Length - 1) + last;
                return previous;
            }
        }
    }
}