using System.Collections.Generic;

namespace Xilium.Crdtp.Tools
{
    public sealed class GenerateCommand
    {
        private IReadOnlyList<string> InputPaths { get; }
        private string OutputPath { get; }
        private string? TargetProjectName { get; }
        private string? TargetNamespaceName { get; }

        public GenerateCommand(
            IReadOnlyList<string> input,
            string outputPath,
            string? targetProjectName,
            string? targetNamespaceName)
        {
            InputPaths = input;
            OutputPath = outputPath;

            // TODO: There is one of sucky case, when System.CommandLine is just bad.
            // Missing options might provide some default, but somewhy it defaults to empty string, instead of null,
            // even if explicitly specified. Absense of option, and presense of option with empty value is absolutely
            // independent things. So, currently just treat empties as nulls.
            TargetProjectName = string.IsNullOrEmpty(targetProjectName) ? null : targetProjectName;
            TargetNamespaceName = string.IsNullOrEmpty(targetNamespaceName) ? null : targetNamespaceName;
        }

        public int Execute()
        {
            // TODO: check InputPaths more gently
            // TODO: check OutputPath
            Check.That(InputPaths.Count > 0);
            Check.That(!string.IsNullOrEmpty(OutputPath));

            var options = new ClientGeneratorOptions
            {
                InputFiles = InputPaths,
                OutputPath = OutputPath,

                Namespace = TargetNamespaceName ?? TargetProjectName ?? "Xilium.Crdtp.Protocol",
                EmitDocumentation = true,

                Stj = new StjSerializationOptions
                {
                    Enabled = true,
                    CamelCaseNamingConvention = true,
                },

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
