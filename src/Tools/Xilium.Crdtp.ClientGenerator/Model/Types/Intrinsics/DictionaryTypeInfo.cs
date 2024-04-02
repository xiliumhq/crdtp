namespace Xilium.Crdtp.Model
{
    internal sealed class DictionaryTypeInfo : IntrinsicTypeInfo
    {
        public DictionaryTypeInfo(Context context)
            : base(context)
        { }

        public override string Name => "protocol_object";
        public override string GetFullyQualifiedName()
            => "System.Collections.Generic.Dictionary<string, object>"; // CrdtpDictionary? Dictionary<string, object> ?
        public override string GetTypeInfoPropertyName()
        {
            return "DictionaryOfStringObject";
        }

        public override bool IsValueType => false;
    }
}
