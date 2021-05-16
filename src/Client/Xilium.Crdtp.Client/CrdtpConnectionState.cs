namespace Xilium.Crdtp.Client
{
    public enum CrdtpConnectionState
    {
        None = 0,
        Connecting,
        Connected,
        Open,
        Closing,
        Closed,
        Aborted,
    }
}
