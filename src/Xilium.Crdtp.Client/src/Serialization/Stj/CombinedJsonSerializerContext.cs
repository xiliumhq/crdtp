using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Xilium.Crdtp.Client.Serialization;

// TODO: Remove me
internal sealed class CombinedJsonSerializerContext : JsonSerializerContext
{
    private readonly JsonSerializerContext[] _contexts;

    public CombinedJsonSerializerContext(JsonSerializerOptions? options, JsonSerializerContext[] contexts)
        : base(options)
    {
        _contexts = contexts;
    }

    protected override JsonSerializerOptions? GeneratedSerializerOptions
        => Options;

    public override JsonTypeInfo? GetTypeInfo(Type type)
    {
        foreach (var c in _contexts)
        {
            var result = c.GetTypeInfo(type);
            if (result != null)
                return result;
        }
        return null;
    }
}
