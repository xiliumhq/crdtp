using System.Collections.Immutable;

namespace Xilium.Crdtp.Sema.Symbols
{
    public abstract class ProtocolSymbol : Symbol
    {
        public abstract ProtocolVersion Version { get; }

        public abstract ImmutableArray<DomainSymbol> Domains { get; }
    }
}
