namespace Xilium.Chromium.DevTools.Syntax
{

    using System;

    [Flags]
    public enum TypeModifiers
    {
        None = 0,

        // Access Modifiers
        Public = 1 << 0,

        Protected = 1 << 1,
        Private = 1 << 2,
        Internal = 1 << 3,

        // ???
        Static = 1 << 4,

        Partial = 1 << 5,
        Abstract = 1 << 6,
        Sealed = 1 << 7,

        ReadOnly = 1 << 8,

        [Obsolete("This is not correct. Property modifiers are other.")]
        Event = 1 << 9,
    }
}
