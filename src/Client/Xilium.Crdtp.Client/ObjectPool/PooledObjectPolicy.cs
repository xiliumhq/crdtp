namespace Xilium.Crdtp.ObjectPool
{
    internal abstract class PooledObjectPolicy<T>
        where T : notnull
    {
        public abstract T Create();

        public abstract bool Return(T obj);
    }
}
