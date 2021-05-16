using System.Collections.Immutable;
using System.Linq;
using Xilium.Crdtp.Emitters;
using Xilium.Crdtp.Sema.Symbols;

namespace Xilium.Crdtp.Model
{
    internal sealed class SyntheticObjectTypeInfo : ObjectTypeInfo
    {
        private readonly Kind _kind;
        private readonly MemberInfo _containingMember;
        private readonly ImmutableArray<PropertyInfo> _properties;

        public SyntheticObjectTypeInfo(Context context,
            Kind kind,
            MemberInfo containingMember,
            ImmutableArray<PropertySymbol> properties)
            : base(context)
        {
            if (kind == Kind.CommandParametersType && containingMember is CommandInfo) { }
            else if (kind == Kind.CommandReturnType && containingMember is CommandInfo) { }
            else if (kind == Kind.EventParametersType && containingMember is EventInfo) { }
            else throw Error.Argument(nameof(kind));
            Check.That(properties.Length > 0);

            _kind = kind;
            _containingMember = containingMember;
            _properties = properties.Select(x => new PropertyInfo(context, this, x)).ToImmutableArray();
        }

        public override DomainInfo? Domain => _containingMember.Domain;

        public override string Name => GetName();

        public override string ProtocolName => throw Error.InvalidOperation();

        public override ImmutableArray<PropertyInfo> Properties => _properties;

        public override bool IsExperimental => false;

        public override bool IsDeprecated => false;

        public override ImmutableArray<string> Description => ImmutableArray<string>.Empty;

        internal override Emitter? GetEmitter()
        {
            return new ObjectTypeEmitter(Context, this);
        }

        private string GetName()
        {
            var name = _containingMember.Name;

            var suffix = _kind switch
            {
                Kind.CommandParametersType => Context.Options.CommandRequestTypeSuffix,
                Kind.CommandReturnType => Context.Options.CommandResponseTypeSuffix,
                Kind.EventParametersType => Context.Options.EventTypeSuffix,
                _ => throw Error.Unreachable(),
            };

            return name + suffix;
        }

        public enum Kind
        {
            CommandParametersType = 1,
            CommandReturnType,
            EventParametersType,
        }
    }
}
