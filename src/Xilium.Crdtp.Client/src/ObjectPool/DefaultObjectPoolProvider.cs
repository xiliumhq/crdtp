using System;

namespace Xilium.Crdtp.ObjectPool
{
    internal class DefaultObjectPoolProvider : ObjectPoolProvider
    {
        public int MaximumRetained { get; set; } = Environment.ProcessorCount * 2;

        public override ObjectPool<T> Create<T>(PooledObjectPolicy<T> policy)
        {
            Check.Argument.NotNull(policy, nameof(policy));

            // TODO(dmitry.azaraev): (DisposableObjectPool) is not implemented, but it is currently not used.
            DebugCheck.That(!typeof(IDisposable).IsAssignableFrom(typeof(T)));

            return new DefaultObjectPool<T>(policy, MaximumRetained);
        }
    }
}
