namespace Xilium.Crdtp.Model
{
    internal sealed class NumberTypeInfo : IntrinsicTypeInfo
    {
        public NumberTypeInfo(Context context)
            : base(context)
        { }

        public override string Name => "protocol_number";

        public override string GetFullyQualifiedName() => "double";

        public override bool IsValueType => true;
    }
}
