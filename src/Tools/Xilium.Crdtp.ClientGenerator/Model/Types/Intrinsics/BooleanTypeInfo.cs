namespace Xilium.Crdtp.Model
{
    internal sealed class BooleanTypeInfo : IntrinsicTypeInfo
    {
        public BooleanTypeInfo(Context context)
            : base(context)
        { }

        public override string Name => "protocol_boolean";
        public override string GetFullyQualifiedName() => "bool";

        public override bool IsValueType => true;
    }
}
