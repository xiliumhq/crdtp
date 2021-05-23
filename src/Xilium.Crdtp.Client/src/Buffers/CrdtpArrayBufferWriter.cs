#if NET5_0_OR_GREATER
#define HAS_GC_ALLOCATEUNINITIALIZEDARRAY
#endif
#if NETCORE2_0_OR_GREATER || NETSTANDARD21
#define HAS_RUNTIMEHELPER_ISREFERENCEORCONTAINSREFERENCES
#endif

using System;
using System.Buffers;
using System.Diagnostics;

namespace Xilium.Crdtp.Buffers
{
    // TODO(dmitry.azaraev): Instead of resizing array (since it allocate new array anyway),
    // it would be better to return buffer into buffer pool. E.g. create "PooledArrayBufferWriter".

    /// <summary>
    /// Alternative implementation of <see cref="System.Buffers.ArrayBufferWriter{T}" />.
    /// There is two main differences: this implementation uses <c>GC.AllocateUninitializedArray</c>
    /// when this appropriate, and <see cref="CrdtpArrayBufferWriter{T}.Clear"/>
    /// method doesn't zeroing array.
    /// </summary>
    internal sealed class CrdtpArrayBufferWriter<T> : IBufferWriter<T>
    {
        private const int MaxArrayLength = 0X7FEFFFFF;
        private const int DefaultInitialBufferSize = 256;

        private T[] _buffer;
        private int _index;

        public CrdtpArrayBufferWriter()
        {
            _buffer = Array.Empty<T>();
            _index = 0;
        }

        public CrdtpArrayBufferWriter(int initialCapacity)
        {
            if (initialCapacity <= 0)
                throw Error.Argument(nameof(initialCapacity));

            _buffer =
#if HAS_GC_ALLOCATEUNINITIALIZEDARRAY
                GC.AllocateUninitializedArray<T>(initialCapacity)
#else
                new T[initialCapacity]
#endif
                ;
            _index = 0;
        }

        public ReadOnlyMemory<T> WrittenMemory => _buffer.AsMemory(0, _index);
        public ReadOnlySpan<T> WrittenSpan => _buffer.AsSpan(0, _index);
        public ArraySegment<T> WrittenArraySegment => new ArraySegment<T>(_buffer, 0, _index);
        public int WrittenCount => _index;
        public int Capacity => _buffer.Length;
        public int FreeCapacity => _buffer.Length - _index;

        public void Clear()
        {
            Debug.Assert(_buffer.Length >= _index);
#if HAS_RUNTIMEHELPER_ISREFERENCEORCONTAINSREFERENCES
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
#endif
            {
                _buffer.AsSpan(0, _index).Clear();
            }
            _index = 0;
        }

        public void Advance(int count)
        {
            if (count < 0) throw Error.Argument(nameof(count));

            if (_index > _buffer.Length - count)
                ThrowInvalidOperationException_AdvancedTooFar(_buffer.Length);

            _index += count;
        }

        public Memory<T> GetMemory(int sizeHint = 0)
        {
            CheckAndResizeBuffer(sizeHint);
            Debug.Assert(_buffer.Length > _index);
            return _buffer.AsMemory(_index);
        }

        public Span<T> GetSpan(int sizeHint = 0)
        {
            CheckAndResizeBuffer(sizeHint);
            Debug.Assert(_buffer.Length > _index);
            return _buffer.AsSpan(_index);
        }

        public ArraySegment<T> GetArraySegment(int sizeHint = 0)
        {
            CheckAndResizeBuffer(sizeHint);
            Debug.Assert(_buffer.Length > _index);
            return new ArraySegment<T>(_buffer, _index, _buffer.Length - _index);
        }

        private void CheckAndResizeBuffer(int sizeHint)
        {
            if (sizeHint < 0)
                throw Error.Argument(nameof(sizeHint));

            if (sizeHint == 0)
            {
                sizeHint = 1;
            }

            if (sizeHint > FreeCapacity)
            {
                int currentLength = _buffer.Length;

                // Attempt to grow by the larger of the sizeHint and double the current size.
                int growBy = Math.Max(sizeHint, currentLength);

                if (currentLength == 0)
                {
                    growBy = Math.Max(growBy, DefaultInitialBufferSize);
                }

                int newSize = currentLength + growBy;

                if ((uint)newSize > int.MaxValue)
                {
                    // Attempt to grow to MaxArrayLength.
                    uint needed = (uint)(currentLength - FreeCapacity + sizeHint);
                    Debug.Assert(needed > currentLength);

                    if (needed > MaxArrayLength)
                    {
                        ThrowOutOfMemoryException(needed);
                    }

                    newSize = MaxArrayLength;
                }

#if HAS_GC_ALLOCATEUNINITIALIZEDARRAY
                var newBuffer = GC.AllocateUninitializedArray<T>(newSize);
                Array.Copy(_buffer, newBuffer, _index);
                _buffer = newBuffer;
#else
                Array.Resize(ref _buffer, newSize);
#endif
            }

            Debug.Assert(FreeCapacity > 0 && FreeCapacity >= sizeHint);
        }

        private static void ThrowInvalidOperationException_AdvancedTooFar(int capacity)
        {
            throw Error.InvalidOperation("Cannot advance past the end of the buffer, which has a size of {0}.", capacity);
        }

        private static void ThrowOutOfMemoryException(uint capacity)
        {
            throw Error.OutOfMemory("Cannot allocate a buffer of size {0}.", capacity);
        }
    }
}
