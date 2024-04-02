using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Xilium.Crdtp.Client.Serialization;

internal sealed class StjTypeInfoResolver
{
    private IJsonTypeInfoResolver _jsonTypeInfoResolver;
    private JsonSerializerOptions _jsonSerializerOptions;

    private StjSerializationContextFactory? _lastRegisteredFactory;
    private readonly HashSet<StjSerializationContextFactory> _factoriesMap;
    private readonly List<StjSerializationContextFactory> _factories;

    public StjTypeInfoResolver(
        StjSerializationContextFactory initialJsonSerializerContext)
    {
        Check.Argument.NotNull(initialJsonSerializerContext, nameof(initialJsonSerializerContext));

        _factories = new List<StjSerializationContextFactory>();
        _factoriesMap = new HashSet<StjSerializationContextFactory>();

        AddSlow(initialJsonSerializerContext);

        DebugCheck.That(_jsonTypeInfoResolver != null);
        DebugCheck.That(_jsonSerializerOptions != null);
    }

    public JsonSerializerOptions JsonSerializerOptions => _jsonSerializerOptions;

    // TODO: This is not necessary
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public JsonTypeInfo? GetTypeInfo(Type type)
    {
        var result = _jsonTypeInfoResolver.GetTypeInfo(type, _jsonSerializerOptions);
        if (result == null)
            Throw_NoMetadataForType(type);
        return result;
    }

    // TODO: This is not necessary
    public JsonTypeInfo<T> GetTypeInfo<T>()
    {
        var result = _jsonTypeInfoResolver.GetTypeInfo(typeof(T), _jsonSerializerOptions);
        if (result == null)
            Throw_NoMetadataForType(typeof(T));
        return (JsonTypeInfo<T>)result;
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
                BuildJsonTypeInfoResolverAndOptions();
            }
        }

        _lastRegisteredFactory = factory;
    }

    private void BuildJsonTypeInfoResolverAndOptions()
    {
        // Build Options
        var options = StjOptions.CreateJsonSerializerOptions();

        var factories = CollectionsMarshal.AsSpan(_factories);
        for (var i = 0; i < factories.Length; i++)
        {
            var f = factories[i];
            foreach (var converter in f.GetJsonConverters())
            {
                options.Converters.Add(converter);
            }
        }

        var contexts = new List<IJsonTypeInfoResolver>(factories.Length);
        for (var i = 0; i < factories.Length; i++)
        {
            var f = factories[i];
            var context = f.CreateJsonSerializerContext();
            if (context != null)
            {
                contexts.Add(context);
            }
        }

        // #pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
        // // By default fallback to reflection-based resolver.
        // contexts.Add(new DefaultJsonTypeInfoResolver());
        // #pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code

        var typeInfoResolver = JsonTypeInfoResolver.Combine(
            contexts.ToArray());

        options.TypeInfoResolver = typeInfoResolver;

        _jsonSerializerOptions = options;
        _jsonTypeInfoResolver = typeInfoResolver;
    }

    [DoesNotReturn]
    private static InvalidOperationException Throw_NoMetadataForType(Type type)
    {
        throw new InvalidOperationException(
            $"No metadata for type {type.FullName}."
            );
    }
}
