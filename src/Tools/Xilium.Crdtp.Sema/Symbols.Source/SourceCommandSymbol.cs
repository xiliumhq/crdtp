using System.Collections.Generic;
using System.Collections.Immutable;
using Sx = Xilium.Crdtp.Pdl.Syntax;

namespace Xilium.Crdtp.Sema.Symbols.Source
{
    internal sealed class SourceCommandSymbol : CommandSymbol
    {
        private readonly Symbol _containingSymbol;
        private readonly Sx.CommandSyntax _commandSyntax;
        private readonly Scope _scope;
        private readonly ImmutableArray<string> _description;

        private ImmutableArray<PropertySymbol> _parameters;
        private ImmutableArray<PropertySymbol> _returns;

        public SourceCommandSymbol(Symbol containingSymbol, Sx.CommandSyntax commandSyntax, Scope scope)
        {
            _containingSymbol = containingSymbol;
            _commandSyntax = commandSyntax;
            _scope = scope;
            _description = _commandSyntax.Description.ToImmutableArray();
        }

        public override Symbol? ContainingSymbol => _containingSymbol;

        public override string Name => _commandSyntax.Name;

        public override ImmutableArray<PropertySymbol> Parameters => _parameters;

        public override ImmutableArray<PropertySymbol> Returns => _returns;

        public override string Redirect => _commandSyntax.Redirect;

        public override ImmutableArray<string> RedirectDescription => _commandSyntax.RedirectDescription.ToImmutableArray();

        public override bool IsDeprecated => _commandSyntax.IsDeprecated;

        public override bool IsExperimental => _commandSyntax.IsExperimental;

        public override ImmutableArray<string> Description => _description;

        internal void Declare()
        {
            _parameters = CreateProperties(_commandSyntax.Parameters);
            _returns = CreateProperties(_commandSyntax.Returns);
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
