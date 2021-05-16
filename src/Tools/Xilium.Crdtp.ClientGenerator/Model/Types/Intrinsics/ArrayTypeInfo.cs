using Xilium.Crdtp.Sema.Symbols;

namespace Xilium.Crdtp.Model
{
    internal sealed class ArrayTypeInfo : IntrinsicTypeInfo
    {
        private readonly ArrayTypeSymbol _arrayTypeSymbol;
        private readonly TypeInfo _elementType;

        public ArrayTypeInfo(Context context, ArrayTypeSymbol arrayTypeSymbol)
            : base(context)
        {
            _arrayTypeSymbol = arrayTypeSymbol;
            _elementType = Context.GetTypeInfo(arrayTypeSymbol.ElementType);
        }

        public override string Name => "protocol_array_of<" + _arrayTypeSymbol.ElementType.Name + ">";

        public override string GetFullyQualifiedName()
        {
            return "System.Collections.Generic.List<" + ElementType.GetFullyQualifiedName() + ">";
        }

        public TypeInfo ElementType => _elementType;

        public override bool IsValueType => false;

        public override bool Mark(SymbolInfoFlags flags)
        {
            var marked = base.Mark(flags);
            if (marked)
            {
                marked |= _elementType.Mark(flags);
            }
            return marked;
        }
    }
}
