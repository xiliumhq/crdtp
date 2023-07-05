namespace Xilium.Crdtp
{
    public sealed class StjSerializationOptions
    {
        public bool Enabled { get; set; }
        public bool CamelCaseNamingConvention { get; set; }

        /// <summary>
        /// Emit type dependency infromation for dynamic access.
        /// </summary>
        public bool Trimmable { get; set; }

        /// <summary>
        /// Emit property names if assembly will be obfuscated.
        /// </summary>
        public bool Obfuscation { get; set; }
    }
}
