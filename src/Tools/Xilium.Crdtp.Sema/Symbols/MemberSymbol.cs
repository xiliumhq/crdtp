using System.Collections.Immutable;

namespace Xilium.Crdtp.Sema.Symbols
{
    public abstract class MemberSymbol : Symbol
    {
        public abstract bool IsDeprecated { get; }
        public abstract bool IsExperimental { get; }
        public abstract ImmutableArray<string> Description { get; }
    }
}
