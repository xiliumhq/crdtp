using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Xilium.Crdtp.Client.Serialization;

internal sealed class StjTypeInfoResolver
{
    private JsonSerializerOptions _jsonSerializerOptions;

    private StjSerializationContextFactory? _lastRegisteredFactory;
    private readonly HashSet<StjSerializationContextFactory> _factoriesMap;
    private readonly List<IJsonTypeInfoResolver> _contexts;

    public StjTypeInfoResolver(
        StjSerializationContextFactory initialJsonSerializerContext)
    {
        Check.Argument.NotNull(initialJsonSerializerContext, nameof(initialJsonSerializerContext));

        _jsonSerializerOptions = StjOptions.CreateJsonSerializerOptions();

        _factoriesMap = new HashSet<StjSerializationContextFactory>();
        _contexts = new List<IJsonTypeInfoResolver>();

        AddSlow(initialJsonSerializerContext);
    }

    public JsonSerializerOptions JsonSerializerOptions => _jsonSerializerOptions;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(StjSerializationContextFactory factory)
    {
        var lastStjSerializerOptions = _lastRegisteredFactory;
        if (lastStjSerializerOptions == (object)factory)
        {
            return;
        }

        AddSlow(factory);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddSlow(StjSerializationContextFactory factory)
    {
        Check.Argument.NotNull(factory, nameof(factory));

        lock (_factoriesMap)
        {
            if (_factoriesMap.Add(factory))
            {
                Register(factory);
            }
        }

        _lastRegisteredFactory = factory;
    }

    private void Register(StjSerializationContextFactory factory)
    {
        var newConverters = factory.GetJsonConverters();
        var newContext = factory.CreateJsonSerializerContext();

        DebugCheck.That(_jsonSerializerOptions != null);
        var newOptions = new JsonSerializerOptions(_jsonSerializerOptions);

        foreach (var converter in newConverters)
        {
            newOptions.Converters.Add(converter);
        }

        _contexts.Add(newContext);

        var typeInfoResolver = JsonTypeInfoResolver.Combine(
            _contexts.ToArray());

        newOptions.TypeInfoResolver = typeInfoResolver;

        _jsonSerializerOptions = newOptions;
    }

    [DoesNotReturn]
    private static InvalidOperationException Throw_NoMetadataForType(Type type)
    {
        throw new InvalidOperationException(
            $"No metadata for type {type.FullName}."
            );
    }
}
