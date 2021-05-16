﻿namespace Xilium.Crdtp.Client
{
    internal static class CrdtpFeatures
    {
        public const bool EnableLogging = true;
        public const bool LogSend = true;
        public const bool LogRecv = true;

        public static bool IsLogSendEnabled => EnableLogging && LogSend;
        public static bool IsLogRecvEnabled => EnableLogging && LogRecv;
    }
}
