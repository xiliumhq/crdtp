using System;
using System.Collections.Immutable;
using Xilium.Crdtp.Sema.Symbols;

namespace Xilium.Crdtp.Model
{
    public sealed class CommandInfo : MemberInfo
    {
        private readonly CommandSymbol _commandSymbol;
        private readonly TypeInfo _parametersType;
        private readonly TypeInfo _resultType;
        private SymbolInfoFlags _flags;

        public CommandInfo(Context context, CommandSymbol commandSymbol)
            : base(context)
        {
            _commandSymbol = commandSymbol;
            _parametersType = context.CreateSyntheticTypeOrUnitType(
                SyntheticObjectTypeInfo.Kind.CommandParametersType,
                this, _commandSymbol.Parameters);
            _resultType = context.CreateSyntheticTypeOrUnitType(
                SyntheticObjectTypeInfo.Kind.CommandReturnType,
                this, _commandSymbol.Returns);
        }

        public override DomainInfo? Domain => GetDomainInfoOrDefault(_commandSymbol);

        public override string Name => Context.NamingPolicy.GetCommandName(_commandSymbol);

        public override string ProtocolName => _commandSymbol.Name;

        public string ProtocolMethod => Domain!.Name + "." + ProtocolName;

        public TypeInfo ParametersType => _parametersType;
        public TypeInfo ReturnType => _resultType;

        public override bool IsReachable => (_flags & SymbolInfoFlags.Reachable) != 0;

        public override bool IsExperimental => _commandSymbol.IsExperimental;

        public override bool IsDeprecated => _commandSymbol.IsDeprecated;

        public override ImmutableArray<string> Description => _commandSymbol.Description;

        public void MarkReachable()
        {
            if (IsReachable) return;

            // Console.WriteLine($"{nameof(CommandInfo)}::{nameof(MarkReachable)}: Name = {Name}");

            _flags |= SymbolInfoFlags.Reachable;
            Context.MarkReachable(this);

            // When command is emitted - Domain also should be marked for emission.
            // (but not necessary what all command will be emitted)

            var domainSymbol = _commandSymbol.GetDomainSymbolOrThrow();
            var domainInfo = Context.GetDomainInfo(domainSymbol);
            domainInfo.MarkReachable();

            ParametersType.Mark(SymbolInfoFlags.Reachable | SymbolInfoFlags.Serializable);
            ReturnType.Mark(SymbolInfoFlags.Reachable | SymbolInfoFlags.Deserializable);
        }
    }
}
