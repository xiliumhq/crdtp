using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Xilium.Crdtp.Pdl;

internal static class JsonElementExtensions
{
    public static void AssertKind(this JsonElement element, JsonValueKind kind)
    {
        Check.That(element.ValueKind == kind);
    }

    public static T GetValue<T>(this JsonElement element)
    {
        if (typeof(T) == typeof(string))
        {
            Check.That(element.ValueKind == JsonValueKind.String);
            var value = element.GetString()!;
            Check.That(value != null);
            return (T)(object)value;
        }
        else throw new ArgumentException($"{typeof(T)} is not of supported type.", nameof(T));
    }
}
