using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xilium.Crdtp.Client.Serialization;

internal sealed class DefaultJsonSerializationContextFactory : StjSerializationContextFactory
{
    protected internal override JsonConverter[] GetJsonConverters()
    {
        return new JsonConverter[]
        {
            new DoubleJsonConverter(),
            new StringJsonConverter(),
            new FloatJsonConverter(),
            // TODO(dmitry.azaraev): Serializing/Deserializing Unit type is should not be needed, but need do some tests before removal.
            new UnitJsonConverter(),
        };
    }

    protected internal override JsonSerializerContext CreateJsonSerializerContext()
        => new DefaultJsonSerializerContext();
}
