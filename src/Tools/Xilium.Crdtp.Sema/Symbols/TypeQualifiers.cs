using System;

namespace Xilium.Crdtp.Sema.Symbols
{
    [Flags]
    public enum TypeQualifiers
    {
        None = 0,

        Optional = 1 << 0,
    }
}
