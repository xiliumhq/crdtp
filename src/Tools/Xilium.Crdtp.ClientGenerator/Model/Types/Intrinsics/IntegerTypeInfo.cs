namespace Xilium.Crdtp.Model
{
    internal sealed class IntegerTypeInfo : IntrinsicTypeInfo
    {
        public IntegerTypeInfo(Context context)
            : base(context)
        { }

        public override string Name => "protocol_integer";

        public override string GetFullyQualifiedName() => "int";
        public override bool UseInSerializationContext() => false;

        public override bool IsValueType => true;
    }
}
