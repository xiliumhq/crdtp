namespace Xilium.Chromium.DevTools.Syntax
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The base class for a property getter, setter, or initializer
    /// </summary>
    public abstract class AccessorDeclaration
    {
        public AccessorDeclaration(ArrowExpressionClause arrowExpression)
        {
            ArrowExpression = arrowExpression;
            Members = Array.Empty<SyntaxObject>();
        }

        public AccessorDeclaration(TypeModifiers typeModifiers, IEnumerable<SyntaxObject>? members = null)
        {
            TypeModifiers = typeModifiers;
            ArrowExpression = null;
            Members = members ?? Array.Empty<SyntaxObject>();
        }

        public TypeModifiers TypeModifiers { get; private set; }
        public ArrowExpressionClause? ArrowExpression { get; private set; }
        public IEnumerable<SyntaxObject> Members { get; private set; }
        public abstract string Keyword();
    }

    public sealed class GetAccessorDeclaration : AccessorDeclaration
    {
        public GetAccessorDeclaration()
            : base(TypeModifiers.None) { }

        public GetAccessorDeclaration(ArrowExpressionClause arrowExpression)
            : base(arrowExpression) { }

        public GetAccessorDeclaration(TypeModifiers typeModifiers,
            IEnumerable<SyntaxObject>? members = null)
            : base(typeModifiers, members) { }

        public override string Keyword() => "get";
    }

    public sealed class SetAccessorDeclaration : AccessorDeclaration
    {
        public SetAccessorDeclaration()
            : base(TypeModifiers.None) { }

        public SetAccessorDeclaration(ArrowExpressionClause arrowExpression)
            : base(arrowExpression) { }

        public SetAccessorDeclaration(TypeModifiers typeModifiers,
            IEnumerable<SyntaxObject>? members = null)
            : base(typeModifiers, members) { }

        public override string Keyword() => "set";
    }

    public sealed class InitAccessorDeclaration : AccessorDeclaration
    {
        public InitAccessorDeclaration()
            : base(TypeModifiers.None) { }

        public InitAccessorDeclaration(ArrowExpressionClause arrowExpression)
            : base(arrowExpression) { }

        public InitAccessorDeclaration(TypeModifiers typeModifiers, IEnumerable<SyntaxObject>? members = null)
            : base(typeModifiers, members) { }

        public override string Keyword() => "init";
    }

    public sealed class AddAccessorDeclaration : AccessorDeclaration
    {
        public AddAccessorDeclaration()
            : base(TypeModifiers.None) { }

        public AddAccessorDeclaration(ArrowExpressionClause arrowExpression)
            : base(arrowExpression) { }

        public AddAccessorDeclaration(TypeModifiers typeModifiers,
            IEnumerable<SyntaxObject>? members = null)
            : base(typeModifiers, members) { }

        public override string Keyword() => "add";
    }

    public sealed class RemoveAccessorDeclaration : AccessorDeclaration
    {
        public RemoveAccessorDeclaration()
            : base(TypeModifiers.None) { }

        public RemoveAccessorDeclaration(ArrowExpressionClause arrowExpression)
            : base(arrowExpression) { }

        public RemoveAccessorDeclaration(TypeModifiers typeModifiers,
            IEnumerable<SyntaxObject>? members = null)
            : base(typeModifiers, members) { }

        public override string Keyword() => "remove";
    }
}
