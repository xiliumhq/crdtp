namespace Xilium.Chromium.DevTools.Syntax
{

    using System;

    [Flags]
    public enum CSharpModifiers : long
    {
        None = 0,

        // Access Modifiers
        Public = 1 << 0,

        Protected = 1 << 1,
        Private = 1 << 2,
        Internal = 1 << 3,

        //
        Static = 1 << 4,

        Partial = 1 << 5,
        Abstract = 1 << 6,
        Sealed = 1 << 7,

        Async = 1 << 8,
        Const = 1 << 9,
        Event = 1 << 10,
        Extern = 1 << 11,
        New = 1 << 12,
        Override = 1 << 13,
        ReadOnly = 1 << 14,
        Unsafe = 1 << 15,
        Virtual = 1 << 16,
        Volatile = 1 << 17,
    }
}
