namespace Xilium.Crdtp.Model
{
    internal sealed class UnitTypeInfo : IntrinsicTypeInfo
    {
        public UnitTypeInfo(Context context)
            : base(context)
        { }

        public override string Name => "Unit";

        public override string Namespace => "Xilium.Crdtp.Core";
        public override bool UseInSerializationContext() => false;

        public override bool IsValueType => false;
    }
}
