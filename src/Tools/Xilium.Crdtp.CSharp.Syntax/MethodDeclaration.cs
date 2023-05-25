namespace Xilium.Chromium.DevTools.Syntax
{
    using System.Collections.Generic;

    public sealed class MethodDeclaration : SyntaxObject
    {
        public MethodDeclaration(string name,
            IEnumerable<Parameter>? parameters,
            Parameter returnParameter,
            CSharpModifiers modifiers,
            IEnumerable<AttributeDecl>? attributes = null,
            XmlDocumentation? xmlDocumentation = null,
            IEnumerable<SyntaxObject>? members = null)
        {
            Name = name;
            Parameters = parameters.AsSyntaxList();
            ReturnParameter = returnParameter;
            Modifiers = modifiers;
            Attributes = attributes.AsSyntaxList();
            XmlDocumentation = xmlDocumentation;
            Members = members ?? System.Array.Empty<SyntaxObject>();
            ArrowExpression = null;
        }

        public MethodDeclaration(string name,
            IEnumerable<Parameter>? parameters, Parameter returnParameter, // TODO: returnParameter should precede parameter list
            CSharpModifiers modifiers,
            ArrowExpressionClause arrowExpression,
            IEnumerable<AttributeDecl>? attributes = null,
            XmlDocumentation? xmlDocumentation = null)
        {
            Name = name;
            Parameters = parameters.AsSyntaxList();
            ReturnParameter = returnParameter;
            Modifiers = modifiers;
            Attributes = attributes.AsSyntaxList();
            XmlDocumentation = xmlDocumentation;
            Members = System.Array.Empty<SyntaxObject>();
            ArrowExpression = arrowExpression;
        }

        public string Name { get; private set; }

        public CSharpModifiers Modifiers { get; private set; }

        public IEnumerable<Parameter> Parameters { get; private set; }

        public Parameter ReturnParameter { get; private set; }

        public IEnumerable<AttributeDecl> Attributes { get; private set; }

        public XmlDocumentation? XmlDocumentation { get; private set; }

        public IEnumerable<SyntaxObject> Members { get; private set; }

        public ArrowExpressionClause? ArrowExpression { get; private set; }
    }
}
