using System;

namespace Xilium.Crdtp.Model
{
    public abstract class TypeInfo : MemberInfo
    {
        private SymbolInfoFlags _flags;

        protected TypeInfo(Context context)
            : base(context)
        { }

        public abstract override string Name { get; }
        public virtual string Namespace => Context.NamingPolicy.GetNamespaceName(Domain, this);
        public virtual string GetFullyQualifiedName()
        {
            // TODO: return compound name and use name simplifier

            if (string.IsNullOrEmpty(Namespace)) return Name;
            else return Namespace + "." + Name;
        }

        public override bool IsReachable => (_flags & SymbolInfoFlags.Reachable) != 0;
        public bool IsSerializable => (_flags & SymbolInfoFlags.Serializable) != 0;
        public bool IsDeserializable => (_flags & SymbolInfoFlags.Deserializable) != 0;

        protected virtual bool TrackReachability => true;

        public virtual bool IsValueType => throw Error.NotSupported();

        public virtual bool Mark(SymbolInfoFlags flags)
        {
            if ((_flags & flags) == flags) return false;

            // Console.WriteLine($"{nameof(TypeInfo)}::{nameof(Mark)}: Name = {Name} Flags = {flags} ActualFlags = {_flags}");

            _flags |= flags;
            if (TrackReachability && (flags & SymbolInfoFlags.Reachable) != 0)
            {
                Context.MarkReachable(this);
            }

            // Types doesn't reference domain directly, even while them might be declared in.

            return true;
        }
    }
}
