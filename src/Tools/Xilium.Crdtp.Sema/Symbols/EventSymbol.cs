using System.Collections.Immutable;

namespace Xilium.Crdtp.Sema.Symbols
{
    public abstract class EventSymbol : MemberSymbol
    {
        public abstract string Name { get; }
        public abstract ImmutableArray<PropertySymbol> Parameters { get; }
    }
}
