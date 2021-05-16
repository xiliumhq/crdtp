using System.Collections.Immutable;

namespace Xilium.Crdtp.Sema.Symbols
{
    public sealed class ArrayTypeSymbol : TypeSymbol
    {
        private readonly TypeSymbol _elementType;

        internal ArrayTypeSymbol(TypeSymbol elementType)
        {
            _elementType = elementType;
        }

        public TypeSymbol ElementType => _elementType;

        public override string Name => "array of " + _elementType.Name;

        public override bool IsDeprecated => false;
        public override bool IsExperimental => false;
        public override ImmutableArray<string> Description => ImmutableArray<string>.Empty;
    }
}
