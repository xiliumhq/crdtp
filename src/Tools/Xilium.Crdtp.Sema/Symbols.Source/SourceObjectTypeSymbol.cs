using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Sx = Xilium.Crdtp.Pdl.Syntax;

namespace Xilium.Crdtp.Sema.Symbols.Source
{
    internal sealed class SourceObjectTypeSymbol : ObjectTypeSymbol
    {
        private readonly Symbol _containingSymbol;
        private readonly Sx.TypeSyntax _typeSyntax;
        private readonly Scope _scope;
        private readonly ImmutableArray<string> _description;

        private TypeSymbol? _extends;
        private ImmutableArray<PropertySymbol>? _properties;

        public SourceObjectTypeSymbol(Symbol containingSymbol, Sx.TypeSyntax typeSyntax, Scope scope)
        {
            _containingSymbol = containingSymbol;
            _typeSyntax = typeSyntax;
            _scope = scope;
            _description = _typeSyntax.Description.ToImmutableArray();
        }

        public override Symbol? ContainingSymbol => _containingSymbol;

        public override string Name => _typeSyntax.Name;

        public override TypeSymbol Extends => _extends ?? GetExtends();

        public override ImmutableArray<PropertySymbol> Properties => _properties!.Value;

        public override bool IsDeprecated => _typeSyntax.IsDeprecated;

        public override bool IsExperimental => _typeSyntax.IsExperimental;

        public override ImmutableArray<string> Description => _description;

        internal void Declare()
        {
            Check.That(_typeSyntax.Enum.Count == 0);

            var declaringScope = new DeclaringScope();
            var scope = _scope.HideWith(declaringScope);

            var properties = new List<PropertySymbol>();
            foreach (var propertySyntax in _typeSyntax.Properties)
            {
                var propertySymbol = new SourcePropertySymbol(this, propertySyntax, scope);
                properties.Add(propertySymbol);
                declaringScope.Add(propertySymbol.Name, propertySymbol);
                propertySymbol.Declare();
            }
            _properties = properties.ToImmutableArray();
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
