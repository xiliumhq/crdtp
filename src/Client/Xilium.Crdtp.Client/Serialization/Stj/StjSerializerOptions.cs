using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Xilium.Crdtp.Client.Serialization
{
    // TODO(dmitry.azarev): (Low) Doesn't like this interface / their interface name.
    public abstract class StjSerializerOptions
    {
#if DEBUG
        private bool _getConvertersWasCalled;
#endif

        internal ICollection<JsonConverter> GetConverters()
        {
#if DEBUG
            DebugCheck.That(!_getConvertersWasCalled, $"{nameof(StjSerializerOptions)}::{nameof(GetConverters)} method must be called only once.");
            _getConvertersWasCalled = true;
#endif

            return GetConvertersCore();
        }

        protected abstract ICollection<JsonConverter> GetConvertersCore();
    }
}
