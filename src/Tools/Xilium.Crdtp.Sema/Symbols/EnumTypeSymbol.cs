using System.Collections.Immutable;

namespace Xilium.Crdtp.Sema.Symbols
{
    public abstract class EnumTypeSymbol : TypeSymbol
    {
        public abstract TypeSymbol Extends { get; }
        public abstract ImmutableArray<string> Members { get; }
    }
}
