using System.Collections.Immutable;

namespace Xilium.Crdtp.Sema.Symbols
{
    public abstract class ObjectTypeSymbol : TypeSymbol
    {
        public abstract TypeSymbol Extends { get; }
        public abstract ImmutableArray<PropertySymbol> Properties { get; }
    }
}
