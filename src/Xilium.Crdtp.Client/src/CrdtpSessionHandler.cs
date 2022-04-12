using System.Text.Json;

namespace Xilium.Crdtp.Client
{
    public abstract class CrdtpSessionHandler
    {
        protected internal abstract void OnAttach();
        protected internal abstract void OnDetach();

        /// <summary>
        /// This method called for each request before sending request, and can
        /// be used to validate client state to disallow call if needed (most
        /// useful is for handling target's crash state, because server might
        /// queue command instead of reject it immediately). This method should
        /// return <see cref="null"/> for continuing request,
        /// return <see cref="CrdtpErrorResponse"/> object to reject request
        /// or throw exception.
        /// </summary>
        protected internal abstract CrdtpErrorResponse? OnBeforeSend(JsonEncodedText method);
    }
}
