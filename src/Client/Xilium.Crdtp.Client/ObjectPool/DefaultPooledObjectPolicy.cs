namespace Xilium.Crdtp.ObjectPool
{
    internal class DefaultPooledObjectPolicy<T> : PooledObjectPolicy<T>
        where T : class, new()
    {
        public override T Create() => new T();

        public override bool Return(T obj) => true;
    }
}
