using System;
using System.Collections.Immutable;
using Xilium.Crdtp.Sema.Symbols;

namespace Xilium.Crdtp.Model
{
    public sealed class EventInfo : MemberInfo
    {
        private readonly EventSymbol _eventSymbol;
        private readonly TypeInfo _parametersType;
        private SymbolInfoFlags _flags;

        public EventInfo(Context context, EventSymbol eventSymbol)
            : base(context)
        {
            _eventSymbol = eventSymbol;
            _parametersType = context.CreateSyntheticTypeOrUnitType(
                SyntheticObjectTypeInfo.Kind.EventParametersType,
                this, eventSymbol.Parameters);
        }

        public override DomainInfo? Domain => GetDomainInfoOrDefault(_eventSymbol);

        public override string Name => Context.NamingPolicy.GetEventName(_eventSymbol);

        public override string ProtocolName => _eventSymbol.Name;

        public string ProtocolMethod => Domain!.Name + "." + ProtocolName;

        public TypeInfo ParametersType => _parametersType;

        public override bool IsReachable => (_flags & SymbolInfoFlags.Reachable) != 0;

        public override bool IsExperimental => _eventSymbol.IsExperimental;

        public override bool IsDeprecated => _eventSymbol.IsDeprecated;

        public override ImmutableArray<string> Description => _eventSymbol.Description;

        public void MarkReachable()
        {
            if (IsReachable) return;

            // Console.WriteLine($"{nameof(EventInfo)}::{nameof(MarkReachable)}: Name = {Name}");

            _flags |= SymbolInfoFlags.Reachable;
            Context.MarkReachable(this);

            var domainSymbol = _eventSymbol.GetDomainSymbolOrThrow();
            var domainInfo = Context.GetDomainInfo(domainSymbol);
            domainInfo.MarkReachable();

            _parametersType.Mark(SymbolInfoFlags.Reachable | SymbolInfoFlags.Deserializable);
        }
    }
}
