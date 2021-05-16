using System.Collections.Immutable;
using Xilium.Crdtp.Sema.Symbols;

namespace Xilium.Crdtp.Model
{
    internal sealed class WellKnownOf1TypeInfo : TypeInfo
    {
        private readonly string _name;
        private readonly string _namespace;
        private readonly TypeInfo _typeArg0;

        public WellKnownOf1TypeInfo(Context context, string name, string? @namespace, TypeInfo typeArg0)
            : base(context)
        {
            _name = name;
            _namespace = @namespace ?? Context.Options.Namespace;
            _typeArg0 = typeArg0;
        }

        public override string Name => _name;

        public override string Namespace => _namespace;

        public override string GetFullyQualifiedName()
        {
            return Namespace + "." + Name + "<" + _typeArg0.GetFullyQualifiedName() + ">";
        }

        public override string ProtocolName => throw Error.NotSupported();

        public override DomainInfo? Domain => throw Error.NotSupported();

        public override bool IsExperimental => throw Error.NotSupported();

        public override bool IsDeprecated => throw Error.NotSupported();

        public override ImmutableArray<string> Description => throw Error.NotSupported();
    }
}
