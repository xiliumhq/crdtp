using System.Collections.Immutable;
using Xilium.Crdtp.Model;

namespace Xilium.Crdtp.Emitters
{
    internal sealed class WellKnownTypeInfo : TypeInfo
    {
        private readonly string _name;
        private readonly string _namespace;

        public WellKnownTypeInfo(Context context, string name, string? @namespace)
            : base(context)
        {
            _name = name;
            _namespace = @namespace ?? Context.Options.Namespace;
        }

        public override string Name => _name;

        public override string Namespace => _namespace;

        public override string ProtocolName => throw Error.NotSupported();

        public override DomainInfo? Domain => throw Error.NotSupported();

        public override bool IsExperimental => throw Error.NotSupported();

        public override bool IsDeprecated => throw Error.NotSupported();

        public override ImmutableArray<string> Description => throw Error.NotSupported();
    }
}
