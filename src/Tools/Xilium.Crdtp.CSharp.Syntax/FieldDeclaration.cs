namespace Xilium.Chromium.DevTools.Syntax
{

    using System.Collections.Generic;

    public sealed class FieldDeclaration : SyntaxObject
    {

        public FieldDeclaration(
                string name,
                CSharpModifiers modifiers,
                string type,
                string initialValue = null,
                IEnumerable<AttributeDecl> attributes = null,
                XmlDocumentation xmlDocumentation = null
                )
        {
            Name = name;
            Modifiers = modifiers;
            Type = type;
            InitialValue = initialValue;
            Attributes = attributes.AsSyntaxList();
            XmlDocumentation = xmlDocumentation;
        }

        public string Name { get; private set; }

        public CSharpModifiers Modifiers { get; private set; }

        public string Type { get; private set; }

        public string InitialValue { get; private set; }

        public IEnumerable<AttributeDecl> Attributes { get; private set; }

        public XmlDocumentation XmlDocumentation { get; private set; }
    }
}
