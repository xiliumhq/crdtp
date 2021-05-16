using System.Collections.Immutable;
using Xilium.Crdtp.Model;

namespace Xilium.Crdtp.Emitters
{
    internal sealed class DomainApiTypeInfo : TypeInfo
    {
        private readonly DomainInfo _domainInfo;

        public DomainApiTypeInfo(Context context, DomainInfo domainInfo) : base(context)
        {
            _domainInfo = domainInfo;
        }

        public override string Name => Context.NamingPolicy.GetDomainApiTypeName(_domainInfo);

        public override string Namespace => Context.NamingPolicy.GetNamespaceName(_domainInfo);

        public override string ProtocolName => throw Error.NotSupported();

        public override DomainInfo? Domain => _domainInfo;

        public override bool IsExperimental => throw Error.NotSupported();

        public override bool IsDeprecated => throw Error.NotSupported();

        public override ImmutableArray<string> Description => throw Error.NotSupported();
    }
}
