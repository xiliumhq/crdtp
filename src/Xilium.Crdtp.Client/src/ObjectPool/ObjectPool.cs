namespace Xilium.Crdtp.ObjectPool
{
    internal static class ObjectPool
    {
        public static ObjectPool<T> Create<T>(PooledObjectPolicy<T>? policy = null)
            where T : class, new()
        {
            var provider = new DefaultObjectPoolProvider();
            return provider.Create(policy ?? new DefaultPooledObjectPolicy<T>());
        }
    }
}
