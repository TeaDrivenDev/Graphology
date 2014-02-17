using System;
using System.Text.RegularExpressions;

namespace TeaDriven.Graphology.Traversal
{
    public class RootNamespaceTypeExclusion : FuncTypeExclusion
    {
        #region Dependencies

        private readonly string _ns;

        #endregion Dependencies

        #region Constructors

        public RootNamespaceTypeExclusion(string @namespace)
            : base(type =>
                   {
                       @namespace = Regex.Replace(@namespace, @"\.$", "");
                       bool applies = type.FullName.StartsWith(string.Format("{0}.", @namespace));

                       return applies;
                   })
        {
            if (@namespace == null) throw new ArgumentNullException("namespace");

            this._ns = @namespace;
        }

        #endregion Constructors

        #region Configuration

        public string Namespace
        {
            get { return this._ns; }
        }

        #endregion Configuration
    }
}