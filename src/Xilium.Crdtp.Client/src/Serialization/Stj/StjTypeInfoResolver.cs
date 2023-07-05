using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Xilium.Crdtp.Client.Serialization;

internal sealed class StjTypeInfoResolver
{
#if NET7_0_OR_GREATER
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
        var newContext = factory.CreateJsonSerializerContext(null);

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
#else
    private List<JsonSerializerContext> _contexts;

    private StjSerializationContextFactory? _lastRegisteredFactory;
    private readonly HashSet<StjSerializationContextFactory> _factoriesMap;
    private readonly List<StjSerializationContextFactory> _factories;

    public StjTypeInfoResolver(
        StjSerializationContextFactory initialJsonSerializerContext)
    {
        Check.Argument.NotNull(initialJsonSerializerContext, nameof(initialJsonSerializerContext));

        _contexts = new List<JsonSerializerContext>();

        _factoriesMap = new HashSet<StjSerializationContextFactory>();
        _factories = new List<StjSerializationContextFactory>();

        AddSlow(initialJsonSerializerContext);
    }

    // public JsonSerializerOptions JsonSerializerOptions => _jsonSerializerOptions;

    public JsonTypeInfo<T> GetTypeInfo<T>()
    {
        var typeInfo = (JsonTypeInfo<T>?)GetTypeInfo(typeof(T));
        if (typeInfo == null)
            Throw_NoMetadataForType(typeof(T));
        return typeInfo;
    }

    private JsonTypeInfo? GetTypeInfo(Type type)
    {
        var contexts = _contexts;
        foreach (var c in CollectionsMarshal.AsSpan(contexts))
        {
            var result = c.GetTypeInfo(type);
            if (result != null)
                return result;
        }
        return null;
    }

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
                _factories.Add(factory);
                Rebuild();
            }
        }

        _lastRegisteredFactory = factory;
    }

    private void Rebuild()
    {
        var newOptions = StjOptions.CreateJsonSerializerOptions();
        foreach (var factory in _factories)
        {
            foreach (var converter in factory.GetJsonConverters())
            {
                newOptions.Converters.Add(converter);
            }
        }

        var newContexts = new List<JsonSerializerContext>();
        foreach (var f in _factories)
        {
            var context = f.CreateJsonSerializerContext(
                new JsonSerializerOptions(newOptions));
            newContexts.Add(context);
        }

        _contexts = newContexts;
    }

    [DoesNotReturn]
    private static InvalidOperationException Throw_NoMetadataForType(Type type)
    {
        throw new InvalidOperationException(
            $"No metadata for type {type.FullName}."
            );
    }
#endif
}
