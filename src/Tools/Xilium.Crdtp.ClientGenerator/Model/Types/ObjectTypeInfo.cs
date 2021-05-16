using System.Collections.Immutable;
using Xilium.Crdtp.Emitters;

namespace Xilium.Crdtp.Model
{
    public abstract class ObjectTypeInfo : TypeInfo
    {
        protected ObjectTypeInfo(Context context)
            : base(context)
        { }

        public abstract override string Name { get; }

        public abstract ImmutableArray<PropertyInfo> Properties { get; }

        public override bool Mark(SymbolInfoFlags flags)
        {
            var marked = base.Mark(flags);
            if (marked)
            {
                foreach (var propertyInfo in Properties)
                {
                    marked |= propertyInfo.Type.Mark(flags);
                }
            }
            return marked;
        }

        internal abstract Emitter? GetEmitter();
        internal virtual StjConverterEmitter? GetStjConverterEmitter() => null;
    }
}
