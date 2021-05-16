namespace Xilium.Crdtp.Sema.Symbols
{
    public abstract class AliasTypeSymbol : TypeSymbol
    {
        public abstract TypeSymbol Extends { get; }
    }
}
