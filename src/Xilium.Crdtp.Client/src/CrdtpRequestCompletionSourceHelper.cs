using System;
using System.Runtime.CompilerServices;

namespace Xilium.Crdtp.Client
{
    internal static class CrdtpRequestCompletionSourceHelper
    {
        // TODO: (VeryLow) Does it has sense to use module initializer?
#pragma warning disable CA2255 // The 'ModuleInitializer' attribute should not be used in libraries
        [ModuleInitializer] internal static void Initialize() { }
#pragma warning restore CA2255 // The 'ModuleInitializer' attribute should not be used in libraries

        // Use cached delegate out of CrdtpRequest<T> type to avoid excess
        // delegate instantination for each T. This costs single virtual/interface call,
        // but there is a good trade for less allocations and smaller caller's code.
        public static readonly Action<object?> HandleCancelDelegate = HandleCancel;

        private static void HandleCancel(object? state)
        {
            var request = (CrdtpRequest)state!;
            request.Cancel(true);
            // TODO: should cancellation callback dispose CancellationTokenRegistration itself?
            // this looks strange, because registration should go away regardless of registration.
        }
    }
}
