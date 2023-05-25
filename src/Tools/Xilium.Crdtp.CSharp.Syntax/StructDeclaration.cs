namespace Xilium.Chromium.DevTools.Syntax
{
    using System.Collections.Generic;

    public sealed class StructDeclaration : TypeDeclaration
    {
        public StructDeclaration(string name, TypeModifiers modifiers, IEnumerable<SyntaxObject> members,
            string? baseType = null,
            IEnumerable<AttributeDecl>? attributes = null,
            XmlDocumentation? xmlDocumentation = null)
                : base(name, modifiers, attributes, xmlDocumentation)
        {
            Members = members.AsSyntaxList();
            BaseType = baseType;
        }

        public IEnumerable<SyntaxObject> Members { get; private set; }

        public string? BaseType { get; private set; }
    }
}
