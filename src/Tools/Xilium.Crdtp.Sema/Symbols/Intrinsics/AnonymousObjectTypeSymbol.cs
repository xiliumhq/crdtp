using System.Collections.Immutable;

namespace Xilium.Crdtp.Sema.Symbols
{
    internal sealed class AnonymousObjectTypeSymbol : ObjectTypeSymbol
    {
        private readonly TypeSymbol _extends;
        private readonly ImmutableArray<PropertySymbol> _properties;

        public AnonymousObjectTypeSymbol(TypeSymbol extends, ImmutableArray<PropertySymbol> properties)
        {
            _extends = extends;
            _properties = properties;
        }

        public override TypeSymbol Extends => _extends;

        public override ImmutableArray<PropertySymbol> Properties => _properties;

        public override string Name => null!;

        public override bool IsDeprecated => false;

        public override bool IsExperimental => false;

        public override ImmutableArray<string> Description => ImmutableArray<string>.Empty;
    }
}
