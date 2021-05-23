namespace Xilium.Crdtp.Client
{
    public abstract class CrdtpSessionHandler
    {
        protected internal abstract void OnAttach();
        protected internal abstract void OnDetach();
    }
}
