using System;

namespace Xilium.Crdtp.Client
{
    public abstract class CrdtpClientHandler
    {
        protected internal abstract void OnOpen();
        protected internal abstract void OnClose();
        protected internal abstract void OnAbort(Exception? exception);
        protected internal abstract bool OnUnhandledException(Exception? exception);
    }
}
