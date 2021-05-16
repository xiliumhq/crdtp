using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Sx = Xilium.Crdtp.Pdl.Syntax;

namespace Xilium.Crdtp.Sema.Symbols.Source
{
    internal sealed class SourceAnonymousEnumTypeSymbol : EnumTypeSymbol
    {
        private readonly Symbol _containingSymbol;
        private readonly Sx.PropertySyntax _propertySyntax;
        private readonly Scope _scope;
        private readonly ImmutableArray<string> _members;

        private TypeSymbol? _extends;

        public SourceAnonymousEnumTypeSymbol(Symbol containingSymbol, Sx.PropertySyntax propertySyntax, Scope scope)
        {
            _containingSymbol = containingSymbol;
            _propertySyntax = propertySyntax;
            _scope = scope;
            _members = _propertySyntax.Enum.ToImmutableArray();
        }

        public override Symbol? ContainingSymbol => _containingSymbol;

        public override string Name => null!;

        public override TypeSymbol Extends => _extends ?? GetExtends();

        public override ImmutableArray<string> Members => _members;

        public override bool IsDeprecated => _propertySyntax.IsDeprecated;
        public override bool IsExperimental => false;
        public override ImmutableArray<string> Description => ImmutableArray<string>.Empty;

        internal void Declare()
        {
            Check.That(_propertySyntax.IsArray == false);
        }

        private TypeSymbol GetExtends()
        {
            var extendsType = _scope.Bind<TypeSymbol>(_propertySyntax.Type);
            if (_propertySyntax.IsArray)
            {
                extendsType = Compilation!.CreateArrayType(extendsType);
            }
            _extends = extendsType;
            Check.That(extendsType == Compilation?.Intrinsics.String);
            return _extends;
        }
    }
}
