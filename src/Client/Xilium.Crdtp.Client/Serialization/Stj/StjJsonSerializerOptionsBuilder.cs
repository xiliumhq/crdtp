using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Xilium.Crdtp.Client.Serialization
{
    internal sealed class StjJsonSerializerOptionsBuilder
    {
        private JsonSerializerOptions _jsonSerializerOptions;

        private StjSerializerOptions? _lastStjSerializerOptions;
        private readonly HashSet<StjSerializerOptions> _stjSerializerOptionsSet;

        public StjJsonSerializerOptionsBuilder(JsonSerializerOptions options)
        {
            Check.Argument.NotNull(options, nameof(options));

            _jsonSerializerOptions = options;
            _lastStjSerializerOptions = null;
            _stjSerializerOptionsSet = new HashSet<StjSerializerOptions>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JsonSerializerOptions GetOptions() => _jsonSerializerOptions;

        public void Add(StjSerializerOptions options)
        {
            var lastStjSerializerOptions = _lastStjSerializerOptions;
            if (lastStjSerializerOptions != null && lastStjSerializerOptions == (object)options)
            {
                return;
            }

            AddSlow(options);
        }

        private void AddSlow(StjSerializerOptions options)
        {
            lock (_stjSerializerOptionsSet)
            {
                if (_stjSerializerOptionsSet.Add(options))
                {
                    _jsonSerializerOptions = MergeOptions(options);
                }
            }

            _lastStjSerializerOptions = options;
        }

        private JsonSerializerOptions MergeOptions(StjSerializerOptions options)
        {
            var jsonSerializerOptions = new JsonSerializerOptions(_jsonSerializerOptions);
            foreach (var converter in options.GetConverters())
            {
                jsonSerializerOptions.Converters.Add(converter);
            }
            return jsonSerializerOptions;
        }
    }
}
