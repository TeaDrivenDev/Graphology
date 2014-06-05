using System.Text.RegularExpressions;

namespace TeaDriven.Graphology.Traversal
{
    public class ExactNamespaceTypeExclusion : FuncTypeExclusion
    {
        #region Dependencies

        private readonly string _ns;

        #endregion Dependencies

        #region Constructors

        public ExactNamespaceTypeExclusion(string @namespace)
            : base(type =>
                   {
                       @namespace = Regex.Replace(@namespace, @"\.$", "");

                       Regex rx = new Regex(string.Format(@"^{0}.[^\.]*$", @namespace));
                       bool applies = rx.IsMatch(type.FullName);

                       return applies;
                   })
        {
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