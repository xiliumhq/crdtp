namespace Xilium.Crdtp.Client
{
    public enum CrdtpConnectionState
    {
        None = 0,
        Connecting,
        Open,
        Closing,
        Closed,
        Aborted,
    }
}
