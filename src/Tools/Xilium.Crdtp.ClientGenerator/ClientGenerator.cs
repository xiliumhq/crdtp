using System;
using System.Collections.Generic;
using Xilium.Crdtp.Emitters;
using Xilium.Crdtp.Model;
using Xilium.Crdtp.Pdl;
using Xilium.Crdtp.Sema;
using Xilium.Crdtp.Sema.Symbols;
using IO = System.IO;
using Sx = Xilium.Crdtp.Pdl.Syntax;

namespace Xilium.Crdtp
{
    // TODO: Xilium.Crdtp.Tools.<CommandName>
    public sealed class ClientGenerator
    {
        private readonly ClientGeneratorOptions _options;

        public ClientGenerator(ClientGeneratorOptions options)
        {
            Check.That(options.InputFiles != null && options.InputFiles.Count > 0);
            Check.That(!string.IsNullOrEmpty(options.OutputPath));

            _options = options;
        }

        public void Run()
        {
            var compilation = CreateCompilation();

            var context = new Context
            {
                OutputScope = new OutputDirectoryScope(_options.OutputPath, _options.Verbose),
                Options = _options,
            };
            context.NamingPolicy = new DefaultNamingPolicy(context);

            // TODO: Check GPUInfo object properties (they are IDictionary<string, object> or similar for object/dictionary property.
            // TODO: Check CSSComputedStyleProperty - they identical in two domains (DOM and CSS).
            // TODO: Check LayoutTreeSnapshot

            // Pass: Mark Reachable Entites
            foreach (var domainSymbol in compilation.Protocol.Domains)
            {
                var domainInfo = context.GetDomainInfo(domainSymbol);

                if (IsEmitDomain(domainSymbol))
                {
                    domainInfo.MarkReachable();
                }

                foreach (var typeSymbol in domainSymbol.Types)
                {
                    var typeInfo = context.GetTypeInfo(typeSymbol);
                    if (IsEmitType(typeSymbol))
                    {
                        typeInfo.Mark(SymbolInfoFlags.Reachable);
                    }
                }

                foreach (var commandSymbol in domainSymbol.Commands)
                {
                    var commandInfo = context.GetCommandInfo(commandSymbol);
                    if (IsEmitCommand(commandSymbol))
                    {
                        commandInfo.MarkReachable();
                    }
                }

                foreach (var eventSymbol in domainSymbol.Events)
                {
                    var eventInfo = context.GetEventInfo(eventSymbol);
                    if (IsEmitEvent(eventSymbol))
                    {
                        eventInfo.MarkReachable();
                    }
                }
            }

            // Report non-reachable symbols, make it as option,
            // because it is has no sense when we generate only few commands.
            foreach (var domainSymbol in compilation.Protocol.Domains)
            {
                var domainInfo = context.GetDomainInfo(domainSymbol);
                if (!domainInfo.IsReachable)
                {
                    Console.WriteLine("warning: Domain \"{0}\" is not reachable and will not be emitted.",
                        domainInfo.ProtocolName);
                }

                foreach (var typeSymbol in domainSymbol.Types)
                {
                    var typeInfo = context.GetTypeInfo(typeSymbol);
                    if (!typeInfo.IsReachable)
                    {
                        Console.WriteLine("warning: Type \"{0}.{1}\" is not reachable and will not be emitted.",
                            typeInfo.Domain?.ProtocolName, typeInfo.ProtocolName);
                    }
                }

                foreach (var commandSymbol in domainSymbol.Commands)
                {
                    var commandInfo = context.GetCommandInfo(commandSymbol);
                    if (!commandInfo.IsReachable)
                    {
                        Console.WriteLine("warning: Command \"{0}.{1}\" is not reachable and will not be emitted.",
                            commandInfo.Domain?.ProtocolName, commandInfo.ProtocolName);
                    }
                }

                foreach (var eventSymbol in domainSymbol.Events)
                {
                    var eventInfo = context.GetEventInfo(eventSymbol);
                    if (!eventInfo.IsReachable)
                    {
                        Console.WriteLine("warning: Event \"{0}.{1}\" is not reachable and will not be emitted.",
                            eventInfo.Domain?.ProtocolName, eventInfo.ProtocolName);
                    }
                }
            }

            // Pass: Emitting Types
            var stjSerializerOptionsEmitter = new StjSerializerOptionsEmitter(context);

            // Pass: Collect Convertors to emit
            foreach (var typeInfo in context.GetReachableNodes<TypeInfo>())
            {
                // Console.WriteLine($"Emit: Type ({typeInfo.GetType().Name}): {typeInfo.Name}");
                switch (typeInfo)
                {
                    case ObjectTypeInfo x:
                        {
                            var stjConverterEmitter = x.GetStjConverterEmitter();
                            if (stjConverterEmitter != null)
                            {
                                context.AddStjConverterEmitter(x, stjConverterEmitter);
                            }
                        }
                        break;

                    case EnumTypeInfo x:
                        {
                            var stjConverterEmitter = x.GetStjConverterEmitter();
                            if (stjConverterEmitter != null)
                            {
                                context.AddStjConverterEmitter(x, stjConverterEmitter);
                            }
                        }
                        break;

                    case AliasTypeInfo x:
                        {
                            var stjConverterEmitter = x.GetStjConverterEmitter();
                            if (stjConverterEmitter != null)
                            {
                                context.AddStjConverterEmitter(x, stjConverterEmitter);
                            }
                        }
                        break;

                    case IntrinsicTypeInfo x:
                        // no-op
                        break;

                    default: throw Error.NotImplemented("Invalid type info type: {0}", typeInfo.GetType().Name);
                }
            }

            // Pass: Emitting Types
            foreach (var typeInfo in context.GetReachableNodes<TypeInfo>())
            {
                // Console.WriteLine($"Emit: Type ({typeInfo.GetType().Name}): {typeInfo.Name}");
                switch (typeInfo)
                {
                    case ObjectTypeInfo x:
                        {
                            var emitter = x.GetEmitter();
                            if (emitter != null)
                            {
                                emitter.Emit();
                            }
                        }
                        break;

                    case EnumTypeInfo x:
                        {
                            var emitter = x.GetEmitter();
                            if (emitter != null)
                            {
                                emitter.Emit();
                            }
                        }
                        break;

                    case AliasTypeInfo x:
                        {
                            var emitter = x.GetEmitter();
                            if (emitter != null)
                            {
                                emitter.Emit();
                            }
                        }
                        break;

                    case IntrinsicTypeInfo x:
                        // no-op
                        break;

                    default: throw Error.NotImplemented("Invalid type info type: {0}", typeInfo.GetType().Name);
                }
            }

            // Pass: Emitting Serializers
            foreach (var emitter in context.StjConverterEmitters)
            {
                emitter.Emit();
            }
            stjSerializerOptionsEmitter.Emit();

            // Pass: Emitting "DomainApi" types
            foreach (var domainInfo in context.GetReachableNodes<DomainInfo>())
            {
                new DomainApiEmitter(context, domainInfo).Emit();
            }

            // Pass: Emitting "ProtocolApi" type
            new ProtocolApiEmitter(context, stjSerializerOptionsEmitter).Emit();

            // Writing "Protocol.g.props" file.
            new ProjectEmitter(context).Emit();

            Console.WriteLine("Writing files...");
            var outputItemsUpdated = context.OutputScope.Commit();
            // TODO: Write `.crdtp.g.log` file to able remove files which no more in commit / or track file changes more intelligently.

            Console.WriteLine("Done:");
            Console.WriteLine("    Total Items: {0}", context.OutputScope.Items.Count);
            Console.WriteLine("  Updated Items: {0}", outputItemsUpdated);
        }

        private Compilation CreateCompilation()
        {
            var syntaxTrees = new List<Sx.ProtocolSyntax>();
            foreach (var path in _options.InputFiles)
            {
                using var stream = IO.File.OpenRead(path);

                Sx.ProtocolSyntax syntaxTree;
                if (path.EndsWith(".pdl", StringComparison.OrdinalIgnoreCase))
                {
                    using var reader = new IO.StreamReader(stream);
                    syntaxTree = PdlReader.Parse(reader);
                }
                else if (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    var jsonReader = new JsonReader(path);
                    syntaxTree = jsonReader.ParseStream(stream);
                }
                else
                {
                    throw new InvalidOperationException("Generator only accepts `.pdl` or `.json` file extensions.");
                }
                syntaxTrees.Add(syntaxTree);
            }

            var compilation = Compilation.Create(syntaxTrees);
            return compilation;
        }

        private bool IsEmitDomain(DomainSymbol domainSymbol)
        {
            var filter = _options.DomainFilter;
            if (filter == null) return true;
            return filter(domainSymbol);
        }

        private bool IsEmitType(TypeSymbol typeSymbol)
        {
            var filter = _options.TypeFilter;
            if (filter == null) return true;
            return filter(typeSymbol);
        }

        private bool IsEmitCommand(CommandSymbol commandSymbol)
        {
            var filter = _options.CommandFilter;
            if (filter == null) return true;
            return filter(commandSymbol);
        }

        private bool IsEmitEvent(EventSymbol eventSymbol)
        {
            var filter = _options.EventFilter;
            if (filter == null) return true;
            return filter(eventSymbol);
        }
    }
}
