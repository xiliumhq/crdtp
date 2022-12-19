using System;

namespace Xilium.Crdtp.Client
{
    public abstract class CrdtpClientHandler
    {
        protected internal abstract void OnOpen();
        protected internal abstract void OnClose();
        protected internal abstract void OnAbort(Exception? exception);

        // TODO: Describe this better.
        /// <summary>
        /// Return <see langword="false"/> if exception is unhanded. As result
        /// client will be aborted (equivalent of <c>CrdtpClient::Abort(exception)</c>
        /// will be called.
        ///
        /// Return <see langword="true"/> if exception is handled and client
        /// should continue to work (e.g. client ignores exception).
        /// </summary>
        protected internal abstract bool OnUnhandledException(Exception? exception);
    }
}
