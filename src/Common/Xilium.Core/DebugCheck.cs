using System;
using System.Diagnostics;

namespace Xilium
{
    // Note: Do not annotate debug checks with DoesNotReturnIf or similar code
    // analysis attributes, as they doesn't affect behavior for non-debug builds.

    // TODO: DebugCheck should throw own exception type (instead of InvalidOperationException),
    // because debug checks should not be observable by normal flow.

#if XI_CORE_PUBLIC
    public
#else
    internal
#endif
    static partial class DebugCheck
    {
        [Conditional("DEBUG"), DebuggerHidden]
        public static void That(bool value)
        {
            if (!value) throw new InvalidOperationException("Check failed.");
        }

        [Conditional("DEBUG"), DebuggerHidden]
        public static void That(bool value, string message)
        {
            if (!value) throw new InvalidOperationException(message);
        }

        [Conditional("DEBUG"), DebuggerHidden]
        public static void Equal(int actual, int expected)
        {
            if (actual != expected)
                throw new InvalidOperationException("Check failed.");
        }

        [Conditional("DEBUG"), DebuggerHidden]
        public static void Unreachable()
        {
            throw new InvalidOperationException("Unreachable.");
        }
    }
}
