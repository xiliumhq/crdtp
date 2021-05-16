namespace Xilium.Crdtp.Model
{
    internal sealed class AnyTypeInfo : IntrinsicTypeInfo
    {
        public AnyTypeInfo(Context context)
            : base(context)
        { }

        public override string Name => "protocol_any"; // TODO: CrdtpValue? (object currently)
        public override string GetFullyQualifiedName() => "object"; // TODO: use CrdtpValue?

        public override bool IsValueType => false;
    }
}
