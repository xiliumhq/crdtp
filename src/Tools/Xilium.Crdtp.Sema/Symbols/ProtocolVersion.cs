namespace Xilium.Crdtp.Sema.Symbols
{
    public sealed class ProtocolVersion
    {
        private readonly string _major;
        private readonly string _minor;

        public ProtocolVersion(string major, string minor)
        {
            _major = major;
            _minor = minor;
        }

        public string Major => _major;
        public string Minor => _minor;
    }
}
