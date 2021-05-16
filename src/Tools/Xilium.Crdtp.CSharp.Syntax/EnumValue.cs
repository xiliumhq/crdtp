namespace Xilium.Chromium.DevTools.Syntax
{

    using System.Collections.Generic;

    public sealed class EnumValue : SyntaxObject
    {

        public EnumValue(string name, string value = null, IEnumerable<AttributeDecl> attributes = null, XmlDocumentation xmlDocumentation = null)
        {
            Name = name;
            Value = value;
            Attributes = attributes.AsSyntaxList();
            XmlDocumentation = xmlDocumentation;
        }

        public string Name { get; private set; }

        public string Value { get; private set; }

        public IEnumerable<AttributeDecl> Attributes { get; private set; }

        public XmlDocumentation XmlDocumentation { get; private set; }
    }
}
