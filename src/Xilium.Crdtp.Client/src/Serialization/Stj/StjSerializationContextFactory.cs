using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xilium.Crdtp.Client.Serialization;

public abstract class StjSerializationContextFactory
{
    protected internal abstract JsonConverter[] GetJsonConverters();
    protected internal abstract JsonSerializerContext CreateJsonSerializerContext(JsonSerializerOptions? options);
}
