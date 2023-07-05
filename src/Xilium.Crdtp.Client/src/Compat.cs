using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Xilium.Crdtp.Client
{
    internal static class Compat
    {
#if !NET5_0_OR_GREATER
        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key, TValue value)
        {
            if (self.ContainsKey(key)) return false;
            self.Add(key, value);
            return true;
        }
#endif

#if XI_CRDTP_TRIMMABLE_DYNAMIC
        internal const DynamicallyAccessedMemberTypes ForDeserialization
            = DynamicallyAccessedMemberTypes.PublicConstructors
            | DynamicallyAccessedMemberTypes.PublicProperties;

        internal const DynamicallyAccessedMemberTypes ForSerialization
            = DynamicallyAccessedMemberTypes.PublicProperties;
#endif
    }
}
