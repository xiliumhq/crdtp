using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Xilium.Crdtp.Tools;

namespace Xilium.Crdtp.Cli
{
    // TODO: fetching chromium's PDLs
    //private static readonly string[][] Urls = {
    //        new[]{"https://raw.githubusercontent.com/chromium/chromium/master/third_party/blink/public/devtools_protocol/browser_protocol.pdl",
    //        "https://raw.githubusercontent.com/v8/v8/master/include/js_protocol.pdl"},
    //
    //        new[]{"https://chromium.googlesource.com/chromium/src/+/refs/heads/master/third_party/blink/public/devtools_protocol/browser_protocol.pdl?format=TEXT",
    //            "https://chromium.googlesource.com/v8/v8.git/+/refs/heads/master/include/js_protocol.pdl?format=TEXT"}
    //        };

    // cros_protocol.pdl - looks like chromeos specific

    // TODO: picking files from chromium checkout
    // chromium/src
    //   + chrome/browser/devtools/cros_protocol.pdl  -  this is optional
    //   + third_party/blink/public/devtools_protocol/browser_protocol.pdl
    //   + v8/include/js_protocol.pdl
    //   - third_party/devtools-frontend/src/node_modules/devtools-protocol/pdl/browser_protocol.pdl
    //   - third_party/devtools-frontend/src/node_modules/devtools-protocol/pdl/js_protocol.pdl
    //   - third_party/devtools-frontend/src/third_party/blink/public/devtools_protocol/browser_protocol.pdl
    //   - third_party/devtools-frontend/src/v8/include/js_protocol.pdl


    internal static class Program
    {
        private static int Main(string[] args)
        {
            // TODO: Don't use System.CommandLine.

            var result = BuildCommandLine()
                .UseCli()
                .Build()
                .Invoke(args);

            return result;
        }

        private static CommandLineBuilder BuildCommandLine()
        {
            var rootCommand = new RootCommand("A tool for generating crdtp protocol bindings.")
            {
                CreateGenerateCommand()
            };
            rootCommand.Name = "xi-crdtp";
            // rootCommand.AddGlobalOption(new Option<LogEventLevel>("--log-level", () => LogEventLevel.Information, "Logging level"));
            return new CommandLineBuilder(rootCommand);
        }

        private static Command CreateGenerateCommand()
        {
            var command = new Command("generate", "Generate crdtp protocol bindings from PDL/JSON files")
            {
                new Argument<string>("input", "Specifies source (.pdl or .json) files.")
                {
                    Arity = ArgumentArity.OneOrMore,
                },

                new Option<string>("--output-path", "Specifies output directory."),

                // TODO(dmitry.azaraev): (Low) generate command requires external configuration file.

                // TODO: 
                // new Option<string?>("--target-project-name", "Specifies target project name."),

                new Option<string?>("--target-namespace-name", "Specifies target namespace name. When omitted, defaults to target's project name."),

                new Option<bool?>("--target-emit-partial-types", () => false),

                new Option<string?>("--command-request-type-suffix"),
                new Option<string?>("--command-response-type-suffix"),
                new Option<string?>("--event-type-suffix"),

                new Option<string?>("--command-request-anonymous-type-prefix"),
                new Option<string?>("--command-response-anonymous-type-prefix"),
                new Option<string?>("--event-anonymous-type-prefix"),

            };
            command.AddAlias("gen");

            command.Handler = CommandHandler.Create((GenerateCommand cmd) => cmd.Execute());
            return command;
        }
    }
}
