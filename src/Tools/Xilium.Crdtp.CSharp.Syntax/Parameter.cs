namespace Xilium.Chromium.DevTools.Syntax
{

    using System.Collections.Generic;

    public sealed class Parameter : SyntaxObject
    {

        public Parameter(string name, string type, string defaultValue = null, IEnumerable<AttributeDecl> attributes = null)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
            Attributes = attributes.AsSyntaxList();
        }

        public string Name { get; private set; }

        public string Type { get; private set; }

        public string DefaultValue { get; private set; }

        public IEnumerable<AttributeDecl> Attributes { get; private set; }
    }
}
