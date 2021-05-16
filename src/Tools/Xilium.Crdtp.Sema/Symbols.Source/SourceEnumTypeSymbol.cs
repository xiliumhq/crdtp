using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Sx = Xilium.Crdtp.Pdl.Syntax;

namespace Xilium.Crdtp.Sema.Symbols.Source
{
    internal sealed class SourceEnumTypeSymbol : EnumTypeSymbol
    {
        private readonly Symbol _containingSymbol;
        private readonly Sx.TypeSyntax _typeSyntax;
        private readonly Scope _scope;
        private readonly ImmutableArray<string> _description;
        private readonly ImmutableArray<string> _members;

        private TypeSymbol? _extends;

        public SourceEnumTypeSymbol(Symbol containingSymbol, Sx.TypeSyntax typeSyntax, Scope scope)
        {
            _containingSymbol = containingSymbol;
            _typeSyntax = typeSyntax;
            _scope = scope;
            _description = _typeSyntax.Description.ToImmutableArray();
            _members = _typeSyntax.Enum.ToImmutableArray();
        }

        public override Symbol? ContainingSymbol => _containingSymbol;

        public override string Name => _typeSyntax.Name;

        public override TypeSymbol Extends => _extends ?? GetExtends();

        public override ImmutableArray<string> Members => _members;

        public override bool IsDeprecated => _typeSyntax.IsDeprecated;

        public override bool IsExperimental => _typeSyntax.IsExperimental;

        public override ImmutableArray<string> Description => _description;

        internal void Declare()
        {
            Check.That(_typeSyntax.Enum.Count > 0);
            Check.That(_typeSyntax.Extends == "string");
        }

        private TypeSymbol GetExtends()
        {
            var extendsType = _scope.Bind<TypeSymbol>(_typeSyntax.Extends);
            if (_typeSyntax.IsArray)
            {
                extendsType = Compilation!.CreateArrayType(extendsType);
            }
            _extends = extendsType;
            Check.That(extendsType == Compilation?.Intrinsics.String);
            return _extends;
        }
    }
}
