namespace Xilium.Crdtp.Sema.Symbols
{
    public abstract class Symbol
    {
        public virtual Compilation? Compilation => ContainingSymbol?.Compilation;
        public virtual Symbol? ContainingSymbol => null;

        // public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => throw Error.NotImplemented();

        public DomainSymbol? DomainSymbol => GetDomainSymbolOrDefault();

        public DomainSymbol GetDomainSymbolOrThrow()
        {
            var result = GetDomainSymbolOrDefault();
            Check.That(result != null, "DomainSymbol not found.");
            return result;
        }

        private DomainSymbol? GetDomainSymbolOrDefault()
        {
            Symbol? n = this;
            while (n != null)
            {
                if (n is DomainSymbol x) return x;
                n = n.ContainingSymbol;
            }
            return default;
        }
    }
}
