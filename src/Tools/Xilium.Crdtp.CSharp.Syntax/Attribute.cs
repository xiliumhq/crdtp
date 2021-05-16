namespace Xilium.Chromium.DevTools.Syntax
{

    using System.Collections.Generic;

    public sealed class AttributeDecl : SyntaxObject
    {

        // TODO: named parameters should be immutable
        public AttributeDecl(string name, IEnumerable<string> parameters = null, IDictionary<string, string> namedParameters = null)
        {
            Name = name;
            Parameters = parameters.AsSyntaxList();
            NamedParameters = namedParameters ?? new Dictionary<string, string>();
        }

        public string Name { get; private set; }

        public IEnumerable<string> Parameters { get; private set; }

        public IDictionary<string, string> NamedParameters { get; private set; }
    }
}
