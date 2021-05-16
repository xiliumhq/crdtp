using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Sx = Xilium.Crdtp.Pdl.Syntax;

namespace Xilium.Crdtp.Sema.Symbols.Source
{
    internal sealed class SourceEventSymbol : EventSymbol
    {
        private readonly Symbol _containingSymbol;
        private readonly Sx.EventSyntax _eventSyntax;
        private readonly Scope _scope;
        private readonly ImmutableArray<string> _description;

        private ImmutableArray<PropertySymbol> _parameters;

        public SourceEventSymbol(Symbol containingSymbol, Sx.EventSyntax eventSyntax, Scope scope)
        {
            _containingSymbol = containingSymbol;
            _eventSyntax = eventSyntax;
            _scope = scope;
            _description = _eventSyntax.Description.ToImmutableArray();
        }

        public override Symbol? ContainingSymbol => _containingSymbol;

        public override string Name => _eventSyntax.Name;

        public override ImmutableArray<PropertySymbol> Parameters => _parameters;

        public override bool IsDeprecated => _eventSyntax.IsDeprecated;

        public override bool IsExperimental => _eventSyntax.IsExperimental;

        public override ImmutableArray<string> Description => _description;

        internal void Declare()
        {
            _parameters = CreateProperties(_eventSyntax.Parameters);
        }

        private ImmutableArray<PropertySymbol> CreateProperties(ICollection<Sx.PropertySyntax> propertySyntaxes)
        {
            var declaringScope = new DeclaringScope();
            var scope = _scope.HideWith(declaringScope);

            var properties = new List<PropertySymbol>();
            foreach (var propertySyntax in propertySyntaxes)
            {
                var propertySymbol = new SourcePropertySymbol(this, propertySyntax, scope);
                properties.Add(propertySymbol);
                declaringScope.Add(propertySymbol.Name, propertySymbol);
                propertySymbol.Declare();
            }

            return properties.ToImmutableArray();
        }

        private ObjectTypeSymbol CreateObjectType(ICollection<Sx.PropertySyntax> propertySyntaxes)
        {
            var properties = CreateProperties(propertySyntaxes);
            return new AnonymousObjectTypeSymbol(Compilation!.Intrinsics.Object, properties);
        }
    }
}
