namespace Xilium.Crdtp.ObjectPool
{
    internal abstract class ObjectPool<T> where T : class
    {
        public abstract T Rent();

        public abstract void Return(T obj);
    }
}
