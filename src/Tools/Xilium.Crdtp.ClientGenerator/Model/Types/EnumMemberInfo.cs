namespace Xilium.Crdtp.Model
{
    public sealed class EnumMemberInfo
    {
        private readonly string _name;
        private readonly string _protocolName;

        public EnumMemberInfo(string name, string protocolName)
        {
            _name = name;
            _protocolName = protocolName;
        }

        public string Name => _name;

        public string ProtocolName => _protocolName;
    }
}
