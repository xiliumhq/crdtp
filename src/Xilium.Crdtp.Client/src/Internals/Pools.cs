using System;
using System.Text.Json;
using Xilium.Crdtp.Buffers;
using Xilium.Crdtp.Client.Serialization;
using Xilium.Crdtp.ObjectPool;

namespace Xilium.Crdtp.Client
{
    // TODO(dmitry.azaraev): (Low) Use module initializer to initialize pools.

    internal static class Pools
    {
        public static readonly DefaultObjectPool<CrdtpArrayBufferWriter<byte>> BufferWriterPool
            = new DefaultObjectPool<CrdtpArrayBufferWriter<byte>>(new BufferWriterPolicy());

        public static readonly DefaultObjectPool<Utf8JsonWriter> Utf8JsonWriterPool
            = new DefaultObjectPool<Utf8JsonWriter>(new Utf8JsonWriterPolicy());

        private sealed class BufferWriterPolicy : PooledObjectPolicy<CrdtpArrayBufferWriter<byte>>
        {
            public override CrdtpArrayBufferWriter<byte> Create()
            {
                return new CrdtpArrayBufferWriter<byte>();
            }

            public override bool Return(CrdtpArrayBufferWriter<byte> obj)
            {
                obj.Clear();
                return obj.Capacity <= 32 * 1024;
            }
        }

        private sealed class Utf8JsonWriterPolicy : PooledObjectPolicy<Utf8JsonWriter>
        {
            private static readonly CrdtpArrayBufferWriter<byte> s_defaultWriter = new CrdtpArrayBufferWriter<byte>();

            public override Utf8JsonWriter Create()
                => new Utf8JsonWriter(s_defaultWriter, StjOptions.WriterOptions);

            public override bool Return(Utf8JsonWriter obj)
            {
                DebugCheck.That(s_defaultWriter.WrittenCount == 0);
                obj.Reset(s_defaultWriter);
                return true;
            }
        }
    }
}
