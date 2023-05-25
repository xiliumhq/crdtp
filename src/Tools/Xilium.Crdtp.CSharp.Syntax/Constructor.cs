namespace Xilium.Chromium.DevTools.Syntax
{

    using System.Collections.Generic;

    public sealed class Constructor : SyntaxObject
    {

        public Constructor(string name, IEnumerable<Parameter>? parameters,
            CSharpModifiers modifiers,
            IEnumerable<AttributeDecl>? attributes = null,
            XmlDocumentation? xmlDocumentation = null,
            IEnumerable<SyntaxObject>? members = null,
            Subconstructor? subconstructor = null)
        {
            Name = name;
            Parameters = parameters.AsSyntaxList();
            Modifiers = modifiers;
            Attributes = attributes.AsSyntaxList();
            XmlDocumentation = xmlDocumentation;
            Members = members ?? System.Array.Empty<SyntaxObject>();
            Subconstructor = subconstructor;
        }

        public string Name { get; private set; }

        public CSharpModifiers Modifiers { get; private set; }

        public IEnumerable<Parameter> Parameters { get; private set; }

        public IEnumerable<AttributeDecl> Attributes { get; private set; }

        public XmlDocumentation? XmlDocumentation { get; private set; }

        public IEnumerable<SyntaxObject> Members { get; private set; }

        public Subconstructor? Subconstructor { get; }
    }
}
