using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;
using Xilium.Crdtp.Pdl.Syntax;

namespace Xilium.Crdtp.Pdl
{
    public sealed partial class PdlReader
    {
        private class ItemKind
        {
            private CommandSyntax? _command;
            public CommandSyntax? Command { get => _command; set { _type = null; _event = null; _command = value; } }
            [MemberNotNullWhen(true, nameof(Command))]
            public bool IsCommand => Command != null;

            private EventSyntax? _event;
            public EventSyntax? Event { get => _event; set { _type = null; _event = value; _command = null; } }
            [MemberNotNullWhen(true, nameof(Event))]
            public bool IsEvent => Event != null;

            private TypeSyntax? _type;
            public TypeSyntax? Type { get => _type; set { _type = value; _event = null; _command = null; } }
            [MemberNotNullWhen(true, nameof(Type))]
            public bool IsType => Type != null;
        }


        //Ported from https://chromium.googlesource.com/chromium/src/+/refs/heads/master/third_party/inspector_protocol/pdl.py
        public static ProtocolSyntax Parse(StreamReader streamReader, string fileName)
        {
            var mapBinaryToString = false;
            var regexOptions = RegexOptions.Singleline | RegexOptions.Compiled;
            var protocol = new ProtocolSyntax();
            var lineNum = 0;
            var nukeDescription = false;
            var description = new Collection<string>();
            DomainSyntax? domain = null;
            var itemKind = new ItemKind(); //command/event/type
            ICollection<PropertySyntax>? subitems = null; //arguments for parameters|returns|properties
            ICollection<string>? enumliterals = null;

            while (true)
            {
                var line = streamReader.ReadLine();
                if (line == null) break;
                lineNum++;
                var trimLine = line.Trim();

                if (nukeDescription)
                {
                    nukeDescription = false;
                    description = new Collection<string>();
                }
                if (trimLine.StartsWith('#'))
                {
                    description.Add(trimLine.Substring(1));
                    continue;
                }
                nukeDescription = true;
                if (trimLine.Length == 0) continue;
                //
                var match = Regex.Match(line, "^(experimental )?(deprecated )?domain (.*)", regexOptions);
                if (match.Success)
                {
                    domain = new DomainSyntax
                    {
                        IsExperimental = match.Groups[1].Success,
                        IsDeprecated = match.Groups[2].Success,
                        Name = match.Groups[3].Value,
                        Description = description
                    };
                    protocol.Domains.Add(domain);
                    continue;
                }

                match = Regex.Match(line, "^include (.*)", regexOptions);
                if (match.Success)
                {
                    var includedFilename = match.Groups[1].Value;
                    if (Path.IsPathRooted(includedFilename))
                    {
                        throw new InvalidOperationException("Only relative paths are supported in includes");
                    }

                    var resolvedPath = Path.GetFullPath(
                        Path.Combine(Path.GetDirectoryName(fileName)!, includedFilename));

                    // TODO: Track source set (e.g. all referenced files)
                    ProtocolSyntax syntaxTree;
                    {
                        using var stream = File.OpenRead(resolvedPath);
                        using var reader = new StreamReader(stream);
                        syntaxTree = Parse(reader, resolvedPath);
                    }

                    foreach (var x in syntaxTree.Domains)
                    {
                        protocol.Domains.Add(x);
                    }
                    continue;
                }

                match = Regex.Match(line, @"^  depends on ([^\s]+)", regexOptions);
                if (match.Success)
                {
                    Check.That(domain != null);
                    domain.Depends.Add(match.Groups[1].Value);
                    continue;
                }

                match = Regex.Match(line, @"^  (experimental )?(deprecated )?type (.*) extends (array of )?([^\s]+)", regexOptions);
                if (match.Success)
                {
                    var t = new TypeSyntax
                    {
                        IsExperimental = match.Groups[1].Success,
                        IsDeprecated = match.Groups[2].Success,
                        Name = match.Groups[3].Value,
                        Description = description,
                        IsArray = match.Groups[4].Success
                    };
                    var type = match.Groups[5].Value;

                    if (string.Equals(Keys.Enum, type))
                        type = PdlTypes.String;
                    else if (mapBinaryToString && string.Equals(PdlTypes.Binary, type))
                        type = PdlTypes.String;
                    //
                    t.Extends = type;
                    itemKind.Type = t;
                    Check.That(domain != null);
                    domain.Types.Add(t);
                    continue;
                }
                // match = re.compile(r'^  (experimental )?(deprecated )?type (.*) '
                //                   r'extends (array of )?([^\s]+)').match(line)
                //if match:
                //  if 'types' not in domain:
                //    domain['types'] = []
                //  item = createItem({'id': match.group(3)}, match.group(1), match.group(2))
                //  assignType(item, match.group(5), match.group(4), map_binary_to_string)
                //  domain['types'].append(item)
                //  continue

                match = Regex.Match(line, @"^  (experimental )?(deprecated )?(command|event) (.*)", regexOptions);
                if (match.Success)
                {
                    if (string.Equals("command", match.Groups[3].Value))
                    {
                        itemKind.Command = new CommandSyntax
                        {
                            IsExperimental = match.Groups[1].Success,
                            IsDeprecated = match.Groups[2].Success,
                            Description = description,
                            Name = match.Groups[4].Value
                        };
                        Check.That(domain != null);
                        domain.Commands.Add(itemKind.Command);
                    }
                    else
                    {
                        itemKind.Event = new EventSyntax
                        {
                            IsExperimental = match.Groups[1].Success,
                            IsDeprecated = match.Groups[2].Success,
                            Description = description,
                            Name = match.Groups[4].Value
                        };
                        Check.That(domain != null);
                        domain.Events.Add(itemKind.Event);
                    }
                    continue;
                }
                //match = re.compile(
                //    r'^  (experimental )?(deprecated )?(command|event) (.*)').match(line)
                //if match:
                //  list = []
                //  if match.group(3) == 'command':
                //    if 'commands' in domain:
                //      list = domain['commands']
                //    else:
                //      list = domain['commands'] = []
                //  else:
                //    if 'events' in domain:
                //      list = domain['events']
                //    else:
                //      list = domain['events'] = []
                //
                //  item = createItem({}, match.group(1), match.group(2), match.group(4))
                //  list.append(item)
                //  continue

                match = Regex.Match(line, @"^      (experimental )?(deprecated )?(optional )?(array of )?([^\s]+) ([^\s]+)", regexOptions);
                if (match.Success)
                {
                    var arg = new PropertySyntax
                    {
                        IsExperimental = match.Groups[1].Success,
                        IsDeprecated = match.Groups[2].Success,
                        IsOptional = match.Groups[3].Success,
                        IsArray = match.Groups[4].Success,
                        Description = description,
                        Name = match.Groups[6].Value,
                    };
                    //assignType
                    var type = match.Groups[5].Value;
                    if (string.Equals("enum", type))
                    {
                        type = PdlTypes.String;
                        enumliterals = arg.Enum;
                    }
                    else if (mapBinaryToString && string.Equals(PdlTypes.Binary, type))
                        type = PdlTypes.String;
                    //
#pragma warning disable CS0612 // Type or member is obsolete
                    arg.TypeKind = PdlTypes.Contains(type) ? TypeKind.Primitive : TypeKind.Reference;
#pragma warning restore CS0612 // Type or member is obsolete
                    arg.Type = type;
                    Check.That(subitems != null);
                    subitems.Add(arg);
                    continue;
                }
                //match = re.compile(
                //    r'^      (experimental )?(deprecated )?(optional )?'
                //    r'(array of )?([^\s]+) ([^\s]+)').match(line)
                //if match:
                //  param = createItem({}, match.group(1), match.group(2), match.group(6))
                //  if match.group(3):
                //    param['optional'] = True
                //  assignType(param, match.group(5), match.group(4), map_binary_to_string)
                //  if match.group(5) == 'enum':
                //    enumliterals = param['enum'] = []
                //  subitems.append(param)
                //  continue

                match = Regex.Match(line, @"^    (parameters|returns|properties)", regexOptions);
                if (match.Success)
                {
                    var key = match.Groups[1].Value;
                    if (itemKind.IsCommand)
                    {
                        subitems = key switch
                        {
                            Keys.Parameters => itemKind.Command.Parameters,
                            Keys.Returns => itemKind.Command.Returns,
                            _ => throw new NotSupportedException($"The command descriptor does not support the '{key}' section"),
                        };
                    }
                    else if (itemKind.IsEvent)
                    {
                        subitems = key switch
                        {
                            Keys.Parameters => itemKind.Event.Parameters,
                            _ => throw new NotSupportedException($"The event descriptor does not support the '{key}' section"),
                        };
                    }
                    else if (itemKind.IsType)
                    {
                        subitems = key switch
                        {
                            Keys.Properties => itemKind.Type.Properties,
                            _ => throw new NotSupportedException($"The type descriptor does not support the '{key}' section")
                        };
                    }
                    continue;
                }
                //match = re.compile(r'^    (parameters|returns|properties)').match(line)
                //if match:
                //  subitems = item[match.group(1)] = []
                //  continue

                match = Regex.Match(line, @"^    enum", regexOptions);
                if (match.Success)
                {
                    if (itemKind.IsType) enumliterals = itemKind.Type.Enum;
                    else throw new NotSupportedException("The type declaration does not support the 'enum' section.");
                    continue;
                }
                //match = re.compile(r'^    enum').match(line)
                //if match:
                //  enumliterals = item['enum'] = []
                //  continue

                match = Regex.Match(line, @"^version", regexOptions);
                if (match.Success)
                {
                    //protocol.Version = new Model.Version();
                    continue;
                }

                match = Regex.Match(line, @"^  major (\d+)", regexOptions);
                if (match.Success && match.Groups[1].Success)
                {
                    protocol.Version.Major = match.Groups[1].Value;
                    continue;
                }

                match = Regex.Match(line, @"^  minor (\d+)", regexOptions);
                if (match.Success && match.Groups[1].Success)
                {
                    protocol.Version.Minor = match.Groups[1].Value;
                    continue;
                }

                match = Regex.Match(line, @"^    redirect ([^\s]+)", regexOptions);
                if (match.Success)
                {
                    if (itemKind.IsCommand)
                    {
                        itemKind.Command.Redirect = match.Groups[1].Value;
                        itemKind.Command.RedirectDescription = description;
                    }
                    else throw new NotSupportedException("Redirect is supported only for commands.");
                    continue;
                }
                //match = re.compile(r'^    redirect ([^\s]+)').match(line)
                //if match:
                //  item['redirect'] = match.group(1)
                //  continue

                //# enum literal
                match = Regex.Match(line, @"^      (  )?[^\s]+$", regexOptions);
                if (match.Success)
                {
                    Check.That(enumliterals != null);
                    enumliterals.Add(trimLine);
                    continue;
                }
                //match = re.compile(r'^      (  )?[^\s]+$').match(line)
                //if match:
                //  # enum literal
                //  enumliterals.append(trimLine)
                //  continue


                throw new ArgumentException($"Error. Illegal token in line {lineNum} : \"{line}\"");
            }
            return protocol;
        }
    }
}
