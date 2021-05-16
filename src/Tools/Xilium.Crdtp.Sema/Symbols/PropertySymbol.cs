namespace Xilium.Crdtp.Sema.Symbols
{
    public abstract class PropertySymbol : MemberSymbol
    {
        public abstract string Name { get; }

        public abstract QualifiedType Type { get; }
    }
}
