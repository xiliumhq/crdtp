namespace Xilium.Crdtp.Model
{
    internal sealed class BinaryTypeInfo : IntrinsicTypeInfo
    {
        public BinaryTypeInfo(Context context)
            : base(context)
        { }

        public override string Name => "protocol_binary";
        public override string GetFullyQualifiedName()
            => "byte[]"; // TODO: Use CrdtpBinaryValue?, also need use external converter?
        public override bool UseInSerializationContext() => false;

        public override bool IsValueType => false;
    }
}
