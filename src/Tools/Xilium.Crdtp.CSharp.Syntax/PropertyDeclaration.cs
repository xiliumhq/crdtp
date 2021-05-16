namespace Xilium.Chromium.DevTools.Syntax
{
    using System;
    using System.Collections.Generic;

    public sealed class PropertyDeclaration : SyntaxObject
    {

        public PropertyDeclaration(
                string name,
                TypeModifiers typeModifiers,
                string type,
                IEnumerable<AttributeDecl> attributes = null,
                XmlDocumentation xmlDocumentation = null
                )
            : this(name,
                    typeModifiers,
                    type,
                    new AccessorDeclaration[] { new GetAccessorDeclaration(), new SetAccessorDeclaration() },
                    attributes,
                    xmlDocumentation
                    )
        {
        }

        public PropertyDeclaration(
            string name,
            TypeModifiers typeModifiers, // TODO: property declaration should use member access modifiers, not TypeModifiers
            string type,
            ArrowExpressionClause arrowExpression,
            IEnumerable<AttributeDecl> attributes = null,
            XmlDocumentation xmlDocumentation = null
        )
        {
            Name = name;
            TypeModifiers = typeModifiers;
            Type = type;
            ArrowExpression = arrowExpression;
            Attributes = attributes.AsSyntaxList();
            XmlDocumentation = xmlDocumentation;
            AccessorList = Array.Empty<AccessorDeclaration>();
        }

        public PropertyDeclaration(
                string name,
                TypeModifiers typeModifiers,
                string type,
                IEnumerable<AccessorDeclaration> accessorList,
                IEnumerable<AttributeDecl> attributes = null,
                XmlDocumentation xmlDocumentation = null
                )
        {
            Name = name;
            TypeModifiers = typeModifiers;
            Type = type;
            AccessorList = accessorList;
            Attributes = attributes.AsSyntaxList();
            XmlDocumentation = xmlDocumentation;
            ArrowExpression = null;
        }


        public string Name { get; private set; }

        public TypeModifiers TypeModifiers { get; private set; }

        public string Type { get; private set; }

        public IEnumerable<AttributeDecl> Attributes { get; private set; }

        public XmlDocumentation XmlDocumentation { get; private set; }

        public ArrowExpressionClause ArrowExpression { get; private set; }

        public IEnumerable<AccessorDeclaration> AccessorList { get; private set; }
    }
}
