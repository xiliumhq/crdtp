using System.Collections.Generic;

namespace Xilium.Crdtp.Tools
{
    public sealed class GenerateCommand
    {
        private IReadOnlyList<string> InputPaths { get; }
        private string OutputPath { get; }
        private string? TargetProjectName { get; }
        private string? TargetNamespaceName { get; }
        private bool? TargetEmitPartialTypes { get; }

        private string? CommandRequestTypeSuffix { get; }
        private string? CommandResponseTypeSuffix { get; }
        private string? EventTypeSuffix { get; }

        private string? CommandRequestAnonymousTypePrefix { get; }
        private string? CommandResponseAnonymousTypePrefix { get; }
        private string? EventAnonymousTypePrefix { get; }

        private bool NonThrowingMethods { get; }

        private bool StjLegacy { get; }
        private bool StjSerializationContext { get; }
        private bool StjTrimmable { get; }
        private bool StjObfuscation { get; }

        private bool Verbose { get; }

        public GenerateCommand(
            IReadOnlyList<string> input,
            string outputPath,
            // string? targetProjectName,
            string? targetNamespaceName,
            bool? targetEmitPartialTypes,
            string? commandRequestTypeSuffix,
            string? commandResponseTypeSuffix,
            string? eventTypeSuffix,
            string? commandRequestAnonymousTypePrefix,
            string? commandResponseAnonymousTypePrefix,
            string? eventAnonymousTypePrefix,
            bool? nonThrowingMethods,
            bool? stjLegacy,
            bool? stjSerializationContext,
            bool? stjTrimmable,
            bool? stjObfuscation,
            bool verbose)
        {
            InputPaths = input;
            OutputPath = outputPath;

            // TODO: There is one of sucky case, when System.CommandLine is just bad.
            // Missing options might provide some default, but somewhy it defaults to empty string, instead of null,
            // even if explicitly specified. Absense of option, and presense of option with empty value is absolutely
            // independent things. So, currently just treat empties as nulls.
            TargetProjectName = null; // string.IsNullOrEmpty(targetProjectName) ? null : targetProjectName;
            TargetNamespaceName = string.IsNullOrEmpty(targetNamespaceName) ? null : targetNamespaceName;
            TargetEmitPartialTypes = targetEmitPartialTypes;

            CommandRequestTypeSuffix = string.IsNullOrEmpty(commandRequestTypeSuffix) ? null : commandRequestTypeSuffix;
            CommandResponseTypeSuffix = string.IsNullOrEmpty(commandResponseTypeSuffix) ? null : commandResponseTypeSuffix;
            EventTypeSuffix = string.IsNullOrEmpty(eventTypeSuffix) ? null : eventTypeSuffix;

            CommandRequestAnonymousTypePrefix = string.IsNullOrEmpty(commandRequestAnonymousTypePrefix) ? null : commandRequestAnonymousTypePrefix;
            CommandResponseAnonymousTypePrefix = string.IsNullOrEmpty(commandResponseAnonymousTypePrefix) ? null : commandResponseAnonymousTypePrefix;
            EventAnonymousTypePrefix = string.IsNullOrEmpty(eventAnonymousTypePrefix) ? null : eventAnonymousTypePrefix;

            StjLegacy = stjLegacy ?? false;
            StjSerializationContext = stjSerializationContext ?? !StjLegacy;
            StjTrimmable = stjTrimmable ?? false;
            StjObfuscation = stjObfuscation ?? false;

            NonThrowingMethods = nonThrowingMethods ?? !StjLegacy;

            Verbose = verbose;
        }

        public int Execute()
        {
            // TODO: check InputPaths more gently
            // TODO: check OutputPath
            Check.That(InputPaths.Count > 0);
            Check.That(!string.IsNullOrEmpty(OutputPath));

            var options = new ClientGeneratorOptions
            {
                Verbose = Verbose,

                InputFiles = InputPaths,
                OutputPath = OutputPath,

                Namespace = TargetNamespaceName ?? TargetProjectName ?? "Xilium.Crdtp.Protocol",
                EmitDocumentation = true,
                EmitPartialTypes = TargetEmitPartialTypes ?? false,

                Stj = new StjSerializationOptions
                {
                    Enabled = true,
                    CamelCaseNamingConvention = true,

                    Legacy = StjLegacy,
                    SerializationContext = StjSerializationContext,
                    Trimmable = StjTrimmable,
                    Obfuscation = StjObfuscation,
                },

                // TODO: make configurable
                CommandRequestTypeSuffix = CommandRequestTypeSuffix ?? "Request",
                CommandResponseTypeSuffix = CommandResponseTypeSuffix ?? "Response",
                EventTypeSuffix = EventTypeSuffix ?? "Event",

                CommandRequestAnonymousTypePrefix = CommandRequestAnonymousTypePrefix,
                CommandResponseAnonymousTypePrefix = CommandResponseAnonymousTypePrefix,
                EventAnonymousTypePrefix = EventAnonymousTypePrefix,

                NonThrowingMethods = NonThrowingMethods,

                // TODO: This should be customizable. Now emitted all commands / events, but domains and types
                // is not emitted by default. For example NetworkCached resource is no more emitted (because it is not in-use).
                DomainFilter = (domainSymbol) => false,
                TypeFilter = (typeSymbol) => false,
                //CommandFilter = (commandSymbol) => commandSymbol.DomainSymbol?.Name == "Browser" || commandSymbol.DomainSymbol?.Name == "Target",
                //EventFilter = (eventSymbol) => eventSymbol.DomainSymbol?.Name == "Browser" || eventSymbol.DomainSymbol?.Name == "Target",

                //DomainFilter = (domainSymbol) => false,
                //TypeFilter = (typeSymbol) => false,
                //CommandFilter = (commandSymbol) => false,
                //EventFilter = (eventSymbol) => false,

                //DomainFilter = (domainSymbol) => domainSymbol.Name == "Animation",
                //TypeFilter = (typeSymbol) => typeSymbol.Name == "Animation",
                //CommandFilter = (commandSymbol) => commandSymbol.Name == "resolveAnimation",
                //EventFilter = (eventSymbol) => eventSymbol.Name == "animationStarted",
            };
            var x = new ClientGenerator(options);
            x.Run();

            // TODO: return non-success error code if error is occur.
            return 0;
        }
    }
}
