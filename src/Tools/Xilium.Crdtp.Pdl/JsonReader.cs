using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xilium.Crdtp.Pdl.Syntax;

namespace Xilium.Crdtp.Pdl;

public sealed class JsonReader
{
    private readonly string? _path;

    public JsonReader(string? path)
    {
        _path = path;
    }

    public ProtocolSyntax ParseStream(Stream stream)
    {
        var document = JsonDocument.Parse(stream, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip,
            MaxDepth = 300,
        });
        stream.Dispose();
        return ReadProtocolSyntax(document.RootElement);
    }

    private ProtocolSyntax ReadProtocolSyntax(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("Parse error.");
        }

        VersionSyntax? versionSyntax = null;
        List<DomainSyntax>? domains = null;

        if (element.TryGetProperty("version", out var versionElement))
        {
            versionSyntax = ReadVersionSyntax(versionElement);
        }

        if (element.TryGetProperty("domains", out var domainsElement))
        {
            domains = ReadListOfDomainSyntax(domainsElement);
        }

        return new ProtocolSyntax(
            versionSyntax ?? new VersionSyntax(),
            domains ?? new List<DomainSyntax>());
    }

    private VersionSyntax ReadVersionSyntax(JsonElement element)
    {
        var major = element.GetProperty("major").GetValue<string>();
        var minor = element.GetProperty("minor").GetValue<string>();
        return new VersionSyntax(major, minor);
    }

    private List<DomainSyntax> ReadListOfDomainSyntax(JsonElement element)
    {
        element.AssertKind(JsonValueKind.Array);

        var result = new List<DomainSyntax>();
        for (var i = 0; i < element.GetArrayLength(); i++)
        {
            var domainElement = element[i];
            var domainSyntax = ReadDomainSyntax(domainElement);
            result.Add(domainSyntax);
        }
        return result;
    }

    private List<TypeSyntax> ReadListOfTypeSyntax(JsonElement element)
    {
        element.AssertKind(JsonValueKind.Array);

        var result = new List<TypeSyntax>();
        for (var i = 0; i < element.GetArrayLength(); i++)
        {
            var itemElement = element[i];
            var item = ReadTypeSyntax(itemElement);
            result.Add(item);
        }
        return result;
    }

    private List<CommandSyntax> ReadListOfCommandSyntax(JsonElement element)
    {
        element.AssertKind(JsonValueKind.Array);

        var result = new List<CommandSyntax>();
        for (var i = 0; i < element.GetArrayLength(); i++)
        {
            var itemElement = element[i];
            var item = ReadCommandSyntax(itemElement);
            result.Add(item);
        }
        return result;
    }

    private List<EventSyntax> ReadListOfEventSyntax(JsonElement element)
    {
        element.AssertKind(JsonValueKind.Array);

        var result = new List<EventSyntax>();
        for (var i = 0; i < element.GetArrayLength(); i++)
        {
            var itemElement = element[i];
            var item = ReadEventSyntax(itemElement);
            result.Add(item);
        }
        return result;
    }

    private List<PropertySyntax> ReadListOfPropertySyntax(JsonElement element)
    {
        element.AssertKind(JsonValueKind.Array);

        var result = new List<PropertySyntax>();
        for (var i = 0; i < element.GetArrayLength(); i++)
        {
            var itemElement = element[i];
            var item = ReadPropertySyntax(itemElement);
            result.Add(item);
        }
        return result;
    }

    private List<string> ReadListOfString(JsonElement element)
    {
        element.AssertKind(JsonValueKind.Array);

        var result = new List<string>();
        for (var i = 0; i < element.GetArrayLength(); i++)
        {
            var itemElement = element[i];
            var item = itemElement.GetValue<string>();
            result.Add(item);
        }
        return result;
    }

    private DomainSyntax ReadDomainSyntax(JsonElement element)
    {
        element.AssertKind(JsonValueKind.Object);

        // MemberSyntax
        var memberSyntax = ReadMemberSyntax(element);

        // DomainSyntax
        var name = element.GetProperty("domain").GetValue<string>();
        var depends = new HashSet<string>();
        if (element.TryGetProperty("dependencies", out var x))
        {
            foreach (var v in x.EnumerateArray())
            {
                depends.Add(v.GetValue<string>());
            }
        }

        List<TypeSyntax> types;
        if (element.TryGetProperty("types", out var typesElement))
        {
            types = ReadListOfTypeSyntax(typesElement);
        }
        else
        {
            types = new List<TypeSyntax>();
        }

        List<CommandSyntax> commands;
        if (element.TryGetProperty("commands", out var commandsElement))
        {
            commands = ReadListOfCommandSyntax(commandsElement);
        }
        else
        {
            commands = new List<CommandSyntax>();
        }

        List<EventSyntax> events;
        if (element.TryGetProperty("events", out var eventsElement))
        {
            events = ReadListOfEventSyntax(eventsElement);
        }
        else
        {
            events = new List<EventSyntax>();
        }

        return new DomainSyntax(
            memberSyntax.Description,
            memberSyntax.IsDeprecated,
            memberSyntax.IsExperimental,
            name,
            depends,
            types,
            commands,
            events);
    }

    private TypeSyntax ReadTypeSyntax(JsonElement element)
    {
        element.AssertKind(JsonValueKind.Object);

        // MemberSyntax
        var memberSyntax = ReadMemberSyntax(element);

        // TypeSyntax
        var name = element.GetProperty("id").GetValue<string>();

        var extendsTypeRef = GetTypeRef(element);
        // var extends = element.GetProperty("type").GetValue<string>();
        var extends = extendsTypeRef.Type;
        var isArray = extendsTypeRef.IsArray;

        List<string> enumMembers;
        if (element.TryGetProperty("enum", out var enumElement))
        {
            enumMembers = ReadListOfString(enumElement);
            Check.That(extendsTypeRef.Type == "string");
        }
        else
        {
            enumMembers = new List<string>();
        }

        List<PropertySyntax> properties;
        if (element.TryGetProperty("properties", out var propertiesElement))
        {
            properties = ReadListOfPropertySyntax(propertiesElement);
        }
        else
        {
            properties = new List<PropertySyntax>();
        }

        return new TypeSyntax(
            memberSyntax.Description,
            memberSyntax.IsDeprecated,
            memberSyntax.IsExperimental,
            name,
            extends,
            isArray,
            enumMembers,
            properties);
    }

    private CommandSyntax ReadCommandSyntax(JsonElement element)
    {
        element.AssertKind(JsonValueKind.Object);

        // NamedMemberWithParametersSyntax
        var nmwps = ReadNamedMemberWithParametersSyntax(element);

        // CommandSyntax
        List<PropertySyntax> returns;
        if (element.TryGetProperty("returns", out var returnsElement))
        {
            returns = ReadListOfPropertySyntax(returnsElement);
        }
        else
        {
            returns = new List<PropertySyntax>();
        }

        string? redirect = null;
        if (element.TryGetProperty("redirect", out var x))
        {
            redirect = x.GetValue<string>();
        }

        // json legacy format can't express redirect's description
        var redirectDescription = new List<string>();

        return new CommandSyntax(
            nmwps.Description,
            nmwps.IsDeprecated,
            nmwps.IsExperimental,
            nmwps.Name,
            nmwps.Parameters,
            returns,
            redirect,
            redirectDescription);
    }

    private EventSyntax ReadEventSyntax(JsonElement element)
    {
        element.AssertKind(JsonValueKind.Object);

        // NamedMemberWithParametersSyntax
        var nmwps = ReadNamedMemberWithParametersSyntax(element);

        return new EventSyntax(
            nmwps.Description,
            nmwps.IsDeprecated,
            nmwps.IsExperimental,
            nmwps.Name,
            nmwps.Parameters);
    }

    private PropertySyntax ReadPropertySyntax(JsonElement element)
    {
        element.AssertKind(JsonValueKind.Object);

        // MemberSyntax
        var memberSyntax = ReadMemberSyntax(element);

        // PropertySyntax
        var name = element.GetProperty("name").GetValue<string>();

        // Get Type
        var typeRef = GetTypeRef(element);
        bool isOptional = GetOptional(element);

        List<string> enumMembers;
        if (element.TryGetProperty("enum", out var enumElement))
        {
            enumMembers = ReadListOfString(enumElement);
            Check.That(typeRef.Type == "string");
        }
        else
        {
            enumMembers = new List<string>();
        }

        return new PropertySyntax(
            memberSyntax.Description,
            memberSyntax.IsDeprecated,
            memberSyntax.IsExperimental,
            name,
            typeRef.Type,
            typeRef.TypeKind,
            typeRef.IsArray,
            isOptional,
            enumMembers);
    }

    private (string Type, bool IsArray, TypeKind TypeKind)
        GetTypeRef(JsonElement element)
    {
        string type;
        bool isArray;
        bool isTypeRef;

        if (element.TryGetProperty("type", out var typeElement))
        {
            type = typeElement.GetValue<string>();
            isTypeRef = false;
        }
        else if (element.TryGetProperty("$ref", out var typeRefElement))
        {
            type = typeRefElement.GetValue<string>();
            isTypeRef = true;
        }
        else throw Error.InvalidOperation("Property should defined type (with `type` or `$ref` property).");

        isArray = type == "array";

        switch (type)
        {
            // Binary probably is not part of legacy protocol definition, but
            // it can be mapped to PDL.
            case "binary":

            case "any":
            case "object":
            case "integer":
            case "boolean":
            case "string":
            case "number":
                if (element.TryGetProperty("items", out var _))
                {
                    throw Error.InvalidOperation("Element has items property, but it is not expected for type = {0}", type);
                }
                break;

            case "array":
                {
                    var itemsElement = element.GetProperty("items");

                    var hasType = false;
                    if (itemsElement.TryGetProperty("type", out var _)
                        || itemsElement.TryGetProperty("$ref", out var _))
                    {
                        hasType = true;
                    }

                    var hasEnum = false;
                    if (itemsElement.TryGetProperty("enum", out var _))
                    {
                        hasEnum = true;
                        Console.WriteLine("warning: Array item spec (enum) is not supported by PDL and will be ignored.");
                    }

                    var hasDescription = false;
                    if (itemsElement.TryGetProperty("description", out var _))
                    {
                        hasDescription = true;
                        Console.WriteLine("warning: Array item spec (description) is not supported by PDL and will be ignored.");
                    }

                    var expectedPropCount = (hasType ? 1 : 0) + (hasEnum ? 1 : 0) +
                        (hasDescription ? 1 : 0);
                    var propCount = itemsElement.EnumerateObject().Count();
                    if (propCount != expectedPropCount)
                    {
                        throw Error.InvalidOperation("Array item spec have unexpected properties.");
                    }

                    {
                        var itemType = GetTypeRef(itemsElement);
                        type = itemType.Type;

                        Check.That(itemType.IsArray == false);
                        Check.That(type != "array");
                    }
                }
                break;

            default:
                if (isTypeRef == false)
                {
                    throw Error.InvalidOperation("Primitive type `{0}` is not handled.", type);
                }
                break;
        }

        TypeKind typeKind = PdlTypes.Contains(type) ? TypeKind.Primitive : TypeKind.Reference; ;
        return (type, isArray, typeKind);
    }

    private (ICollection<string> Description,
        bool IsDeprecated,
        bool IsExperimental,
        string Name,
        ICollection<PropertySyntax> Parameters
        ) ReadNamedMemberWithParametersSyntax(JsonElement element)
    {
        element.AssertKind(JsonValueKind.Object);
        var memberSyntax = ReadMemberSyntax(element);

        var name = element.GetProperty("name").GetValue<string>();
        List<PropertySyntax> parameters;
        if (element.TryGetProperty("parameters", out var parametersElement))
        {
            parameters = ReadListOfPropertySyntax(parametersElement);
        }
        else
        {
            parameters = new List<PropertySyntax>();
        }

        return (
            memberSyntax.Description,
            memberSyntax.IsDeprecated,
            memberSyntax.IsExperimental,
            name,
            parameters
            );
    }

    private (
        ICollection<string> Description,
        bool IsDeprecated,
        bool IsExperimental
    ) ReadMemberSyntax(JsonElement element)
    {
        element.AssertKind(JsonValueKind.Object);

        var description = GetDescription(element);
        var isDeprecated = GetDeprecated(element);
        var isExperimental = GetExperimental(element);

        return (description, isDeprecated, isExperimental);
    }


    private ICollection<string> GetDescription(JsonElement element)
    {
        element.AssertKind(JsonValueKind.Object);
        if (element.TryGetProperty("description", out var x))
        {
            var description = x.GetValue<string>();
            return new string[] { description };
        }
        return Array.Empty<string>();
    }

    private bool GetDeprecated(JsonElement element)
    {
        element.AssertKind(JsonValueKind.Object);
        if (element.TryGetProperty("deprecated", out var x))
        {
            return x.GetBoolean();
        }
        else
            return false;
    }

    private bool GetExperimental(JsonElement element)
    {
        element.AssertKind(JsonValueKind.Object);
        if (element.TryGetProperty("experimental", out var x))
        {
            return x.GetBoolean();
        }
        else
            return false;
    }

    private bool GetOptional(JsonElement element)
    {
        element.AssertKind(JsonValueKind.Object);
        if (element.TryGetProperty("optional", out var x))
        {
            return x.GetBoolean();
        }
        else return false;
    }
}
