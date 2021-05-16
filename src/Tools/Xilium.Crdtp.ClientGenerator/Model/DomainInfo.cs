using System;
using System.Collections.Immutable;
using System.Linq;
using Xilium.Crdtp.Sema.Symbols;

namespace Xilium.Crdtp.Model
{
    public sealed class DomainInfo : MemberInfo
    {
        private readonly DomainSymbol _domainSymbol;
        private SymbolInfoFlags _flags;
        private ImmutableArray<CommandInfo>? _lazyCommands;
        private ImmutableArray<EventInfo>? _lazyEvents;

        public DomainInfo(Context context, DomainSymbol domainSymbol)
            : base(context)
        {
            _domainSymbol = domainSymbol;
        }

        public override string Name => Context.NamingPolicy.GetDomainName(_domainSymbol);

        public override string ProtocolName => _domainSymbol.Name;

        public override DomainInfo? Domain => null;

        /// <summary>
        /// Determines that Domain is referenced, and should be emitted.
        /// </summary>
        public override bool IsReachable => (_flags & SymbolInfoFlags.Reachable) != 0;

        public override bool IsExperimental => _domainSymbol.IsExperimental;

        public override bool IsDeprecated => _domainSymbol.IsDeprecated;

        public override ImmutableArray<string> Description => _domainSymbol.Description;

        public ImmutableArray<CommandInfo> Commands => _lazyCommands ?? GetCommands();
        public ImmutableArray<EventInfo> Events => _lazyEvents ?? GetEvents();

        public void MarkReachable()
        {
            if (IsReachable) return;

            // Console.WriteLine($"{nameof(Domain)}::{nameof(MarkReachable)}: Name = {Name}");

            _flags |= SymbolInfoFlags.Reachable;
            Context.MarkReachable(this);
        }

        private ImmutableArray<CommandInfo> GetCommands()
        {
            var commands = _domainSymbol.Commands.Select(x => Context.GetCommandInfo(x)).ToImmutableArray();
            _lazyCommands = commands;
            return commands;
        }

        private ImmutableArray<EventInfo> GetEvents()
        {
            var events = _domainSymbol.Events.Select(x => Context.GetEventInfo(x)).ToImmutableArray();
            _lazyEvents = events;
            return events;
        }
    }
}
