using System.Collections.Immutable;
using System.Linq;
using Xilium.Crdtp.Emitters;
using Xilium.Crdtp.Sema.Symbols;

namespace Xilium.Crdtp.Model
{
    internal sealed class SymbolObjectTypeInfo : ObjectTypeInfo
    {
        private readonly ObjectTypeSymbol _objectTypeSymbol;
        private readonly ImmutableArray<PropertyInfo> _properties;

        public SymbolObjectTypeInfo(Context context, ObjectTypeSymbol objectTypeSymbol)
            : base(context)
        {
            _objectTypeSymbol = objectTypeSymbol;
            Check.That(objectTypeSymbol.Extends == objectTypeSymbol.Compilation.Intrinsics.Object);
            _properties = objectTypeSymbol.Properties.Select(x => new PropertyInfo(context, this, x)).ToImmutableArray();
        }

        public override DomainInfo? Domain => GetDomainInfoOrDefault(_objectTypeSymbol);

        public override string Name => Context.NamingPolicy.GetTypeName(_objectTypeSymbol);

        public override string ProtocolName => _objectTypeSymbol.Name;

        public override ImmutableArray<PropertyInfo> Properties => _properties;

        public override bool IsExperimental => _objectTypeSymbol.IsExperimental;

        public override bool IsDeprecated => _objectTypeSymbol.IsDeprecated;

        public override ImmutableArray<string> Description => _objectTypeSymbol.Description;

        internal override Emitter? GetEmitter()
        {
            return new ObjectTypeEmitter(Context, this);
        }
    }
}
