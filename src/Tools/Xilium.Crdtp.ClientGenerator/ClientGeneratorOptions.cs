using System;
using System.Collections.Generic;
using Xilium.Crdtp.Sema.Symbols;

namespace Xilium.Crdtp
{
    public sealed class ClientGeneratorOptions
    {
        public bool Verbose { get; set; }

        public IReadOnlyList<string> InputFiles { get; set; } = default!;
        public string OutputPath { get; set; } = default!;

        public string Namespace { get; set; } = "Xilium.Crdtp.Protocol";
        public bool EmitDocumentation { get; set; } = true;
        public bool EmitTypeAnalysis { get; set; } = false;
        public bool EmitPartialTypes { get; set; } = false;

        public StjSerializationOptions Stj { get; set; } = default!; // TODO: apply defaults

        public string ProtocolApiTypeName { get; set; } = "ProtocolApi";

        public string CommandRequestTypeSuffix { get; set; } = "Request";
        public string CommandResponseTypeSuffix { get; set; } = "Response";
        public string EventTypeSuffix { get; set; } = "Event";

        // Same as CommandRequestTypeSuffix, but uses other suffix for implicitly defined types.
        // This options used only for migrating existing codebase.
        public string? CommandRequestAnonymousTypePrefix { get; set; }
        public string? CommandResponseAnonymousTypePrefix { get; set; }
        public string? EventAnonymousTypePrefix { get; set; }

        public string DomainApiSuffix { get; set; } = "Api";
        public string DomainHandlerSuffix { get; set; } = "Dispatcher";

        public string CrdtpSessionTypeName { get; set; } = "Xilium.Crdtp.Client.CrdtpSession";
        public bool UseApi2 { get; set; } = true;
        public bool StructProtocolApi { get; set; } = true;

        // Determines "Entry" points, allowing filter out unnecessary code.
        public Func<DomainSymbol, bool>? DomainFilter { get; set; }
        public Func<TypeSymbol, bool>? TypeFilter { get; set; }
        public Func<CommandSymbol, bool>? CommandFilter { get; set; }
        public Func<EventSymbol, bool>? EventFilter { get; set; }
    }
}
