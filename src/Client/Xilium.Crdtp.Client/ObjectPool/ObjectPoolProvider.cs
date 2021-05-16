namespace Xilium.Crdtp.ObjectPool
{
    internal abstract class ObjectPoolProvider
    {
        public ObjectPool<T> Create<T>() where T : class, new()
        {
            return Create<T>(new DefaultPooledObjectPolicy<T>());
        }

        public abstract ObjectPool<T> Create<T>(PooledObjectPolicy<T> policy) where T : class;
    }
}
