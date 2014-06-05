using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TeaDriven.Graphology.Visualization
{
    public class RecursiveGenericTypeGetTypeNameString : IGetTypeNameString
    {
        #region Dependencies

        private readonly IGetTypeNameString _innerGetTypeNameString;

        #endregion Dependencies

        #region Constructors

        public RecursiveGenericTypeGetTypeNameString(IGetTypeNameString innerGetTypeNameString)
        {
            if (innerGetTypeNameString == null) throw new ArgumentNullException("innerGetTypeNameString");

            this._innerGetTypeNameString = innerGetTypeNameString;
        }

        #endregion Constructors

        #region IGetTypeNameString Members

        public bool For(Type type, out string typeName)
        {
            if (type == null) throw new ArgumentNullException("type");

            bool handled = false;

            if (type.IsGenericType)
            {
                typeName = Regex.Match(type.Name, @"^([^`]*)").Value;

                Type[] genericArguments = type.GetGenericArguments();

                IList<string> genericArgumentNames = new List<string>();

                foreach (Type genericArgument in genericArguments)
                {
                    string genericArgumentName;
                    this._innerGetTypeNameString.For(genericArgument, out genericArgumentName);

                    genericArgumentNames.Add(genericArgumentName);
                }

                string genericArgumentsString = string.Join(", ", genericArgumentNames.ToArray());

                typeName = string.Format("{0}<{1}>", typeName, genericArgumentsString);

                handled = true;
            }
            else
            {
                typeName = "";
            }

            return handled;
        }

        #endregion IGetTypeNameString Members
    }
}