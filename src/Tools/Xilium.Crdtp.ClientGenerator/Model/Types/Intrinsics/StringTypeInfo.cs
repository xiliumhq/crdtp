namespace Xilium.Crdtp.Model
{
    internal sealed class StringTypeInfo : IntrinsicTypeInfo
    {
        public StringTypeInfo(Context context)
            : base(context)
        { }

        public override string Name => "protocol_string";

        public override string GetFullyQualifiedName() => "string";
        public override bool UseInSerializationContext() => false;

        public override bool IsValueType => false;
    }
}
