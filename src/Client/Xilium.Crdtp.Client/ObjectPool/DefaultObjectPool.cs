using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Xilium.Crdtp.ObjectPool
{
    // TODO(dmitry.azaraev): (Low) Redesign ObjectPool (simplify), keep only things which are required.

    internal class DefaultObjectPool<T> : ObjectPool<T>
        where T : class
    {
        private T? _firstItem;
        private readonly ObjectWrapper[] _items;
        private readonly PooledObjectPolicy<T> _policy;

        private const bool _isDefaultPolicy = false;

        public DefaultObjectPool(PooledObjectPolicy<T> policy)
            : this(policy, Environment.ProcessorCount * 2)
        { }

        public DefaultObjectPool(PooledObjectPolicy<T> policy, int maximumRetained)
        {
            _policy = policy ?? throw Error.ArgumentNull(nameof(policy));
            _items = new ObjectWrapper[maximumRetained - 1];
        }

        public override T Rent()
        {
            var item = _firstItem;
            if (item == null || Interlocked.CompareExchange(ref _firstItem, null, item) != item)
            {
                var items = _items;
                for (var i = 0; i < items.Length; i++)
                {
                    item = items[i].Element;
                    if (item != null && Interlocked.CompareExchange(ref items[i].Element, null, item) == item)
                    {
                        return item;
                    }
                }

                item = Create();
            }

            return item;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private T Create() => _policy.Create();

        public override void Return(T obj)
        {
            // TODO(dmitry.azaraev): ObjectPool - support for default policy. Does it needed? Need benchmark.
            if (_isDefaultPolicy || _policy.Return(obj))
            {
                if (_firstItem != null || Interlocked.CompareExchange(ref _firstItem, obj, null) != null)
                {
                    var items = _items;
                    for (var i = 0; i < items.Length && Interlocked.CompareExchange(ref items[i].Element, obj, null) != null; ++i)
                    {
                    }
                }
            }
        }

        // Performance: Use struct wrapper to avoid array-covariance-checks from
        // the runtime when assigning to elements of the array.
        private struct ObjectWrapper
        {
            public T? Element;
        }
    }
}
