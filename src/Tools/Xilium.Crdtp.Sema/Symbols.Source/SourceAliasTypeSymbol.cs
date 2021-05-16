using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Sx = Xilium.Crdtp.Pdl.Syntax;

namespace Xilium.Crdtp.Sema.Symbols.Source
{
    internal sealed class SourceAliasTypeSymbol : AliasTypeSymbol
    {
        private readonly Symbol _containingSymbol;
        private readonly Sx.TypeSyntax _typeSyntax;
        private readonly Scope _scope;
        private readonly ImmutableArray<string> _description;

        private TypeSymbol? _extends;

        public SourceAliasTypeSymbol(Symbol containingSymbol, Sx.TypeSyntax typeSyntax, Scope scope)
        {
            Check.That(typeSyntax.IsAlias);

            _containingSymbol = containingSymbol;
            _typeSyntax = typeSyntax;
            _scope = scope;
            _description = _typeSyntax.Description.ToImmutableArray();
        }

        public override Symbol? ContainingSymbol => _containingSymbol;

        public override string Name => _typeSyntax.Name;

        public override TypeSymbol Extends => _extends ?? GetExtends();

        public override bool IsDeprecated => _typeSyntax.IsDeprecated;

        public override bool IsExperimental => _typeSyntax.IsExperimental;

        public override ImmutableArray<string> Description => _description;

        internal void Declare()
        {
        }

        private TypeSymbol GetExtends()
        {
            var extendsType = _scope.Bind<TypeSymbol>(_typeSyntax.Extends);
            if (_typeSyntax.IsArray)
            {
                extendsType = Compilation!.CreateArrayType(extendsType);
            }
            _extends = extendsType;
            return _extends;
        }
    }
}
