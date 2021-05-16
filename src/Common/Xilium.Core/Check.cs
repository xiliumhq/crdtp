using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Xilium
{
#if XI_CORE_PUBLIC
    public
#else
    internal
#endif
    static partial class Check
    {
        [DebuggerHidden]
        public static void That([DoesNotReturnIf(false)] bool value)
        {
            if (!value) throw new InvalidOperationException("Check failed.");
        }

        [DebuggerHidden]
        public static void That([DoesNotReturnIf(false)] bool value, string message)
        {
            if (!value) throw new InvalidOperationException(message);
        }

        public static void Equal(int actual, int expected)
        {
            if (actual != expected)
                throw new InvalidOperationException("Check failed.");
        }
    }
}
