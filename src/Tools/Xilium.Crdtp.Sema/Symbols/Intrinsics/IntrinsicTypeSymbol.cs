using System.Collections.Immutable;

namespace Xilium.Crdtp.Sema.Symbols
{
    public sealed class IntrinsicTypeSymbol : TypeSymbol
    {
        private readonly IntrinsicTypeKind _kind;
        private readonly string? _name;

        public IntrinsicTypeSymbol(IntrinsicTypeKind kind, string? name)
        {
            _name = name;
            _kind = kind;
        }

        public override string Name => _name!;

        public IntrinsicTypeKind Kind => _kind;

        public override bool IsDeprecated => false;

        public override bool IsExperimental => false;

        public override ImmutableArray<string> Description => ImmutableArray<string>.Empty;
    }
}
