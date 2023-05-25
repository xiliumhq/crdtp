namespace Xilium.Chromium.DevTools.Syntax
{

    using System.Collections.Generic;

    public abstract class TypeDeclaration : SyntaxObject
    {

        public TypeDeclaration(
                string name,
                TypeModifiers modifiers,
                IEnumerable<AttributeDecl>? attributes,
                XmlDocumentation? xmlDocumentation
                )
        {
            Name = name;
            Modifiers = modifiers;
            Attributes = attributes.AsSyntaxList();
            XmlDocumentation = xmlDocumentation;
        }

        public string Name { get; private set; }

        public TypeModifiers Modifiers { get; private set; }

        public IEnumerable<AttributeDecl> Attributes { get; private set; }

        public XmlDocumentation? XmlDocumentation { get; private set; }
    }
}
