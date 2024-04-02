using System.Collections.Immutable;

namespace Xilium.Crdtp.Model
{
    public abstract class IntrinsicTypeInfo : TypeInfo
    {
        public IntrinsicTypeInfo(Context context)
            : base(context)
        { }

        public override string Namespace => default!;

        public override DomainInfo? Domain => default;

        public override string ProtocolName => throw Error.InvalidOperation();

        public override bool IsExperimental => false;

        public override bool IsDeprecated => false;

        public override ImmutableArray<string> Description => ImmutableArray<string>.Empty;

        protected override bool TrackReachability => true;
    }
}
