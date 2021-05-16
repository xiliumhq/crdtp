using System;

namespace Xilium.Crdtp.Client
{
    internal static class CborHelper
    {
        private const int MajorType_TAG = 6;
        private const int MajorType_BYTE_STRING = 2;
        private const int kMajorTypeBitShift = 5;
        private const int kAdditionalInformationMask = 0x1f;

        private const byte kInitialByteForEnvelope = (MajorType_TAG << kMajorTypeBitShift) | (24 & kAdditionalInformationMask);
        private const byte kInitialByteFor32BitLengthByteString = (MajorType_BYTE_STRING << kMajorTypeBitShift) | (26 & kAdditionalInformationMask);

        /// <summary>
        /// Checks whether |message| is a cbor message.
        /// </summary>
        /// <remarks>
        /// See: https://source.chromium.org/chromium/chromium/src/+/master:third_party/inspector_protocol/crdtp/cbor.cc
        /// IsCBORMessage
        /// </remarks>
        public static bool IsCborMessage(ReadOnlySpan<byte> message)
        {
            return message.Length >= 6
                && message[0] == kInitialByteForEnvelope
                && message[1] == kInitialByteFor32BitLengthByteString;
        }
    }
}
