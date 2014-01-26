using System;
using System.Collections.Generic;
using System.Linq;

namespace TeaDriven.Graphology.Visualization
{
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
}