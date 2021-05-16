using System.Collections.Immutable;

namespace Xilium.Crdtp.Sema.Symbols
{
    public abstract class CommandSymbol : MemberSymbol
    {
        public abstract string Name { get; }
        public abstract ImmutableArray<PropertySymbol> Parameters { get; }
        public abstract ImmutableArray<PropertySymbol> Returns { get; }
        public abstract string Redirect { get; }
        public abstract ImmutableArray<string> RedirectDescription { get; }
    }
}
