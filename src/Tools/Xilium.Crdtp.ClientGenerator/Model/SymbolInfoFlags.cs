using System;

namespace Xilium.Crdtp.Model
{
    [Flags]
    public enum SymbolInfoFlags
    {
        None = 0,

        Reachable = 1 << 0,

        Serializable = 1 << 1,
        Deserializable = 1 << 2,
    }
}
