using System.Collections.Immutable;

namespace Xilium.Crdtp.Sema.Symbols
{
    public abstract class DomainSymbol : MemberSymbol
    {
        public abstract string Name { get; }

        public abstract ImmutableArray<DomainSymbol> Depends { get; }

        public abstract ImmutableArray<TypeSymbol> Types { get; }

        public abstract ImmutableArray<CommandSymbol> Commands { get; }

        public abstract ImmutableArray<EventSymbol> Events { get; }
    }
}
