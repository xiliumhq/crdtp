namespace Xilium.Crdtp.Pdl.Syntax
{
    public sealed class VersionSyntax
    {
        // TODO: [Obsolete("Used by PDL parser, but it should not be used in such way.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public VersionSyntax() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public VersionSyntax(string major, string minor)
        {
            Major = major;
            Minor = minor;
        }

        public string Major { get; set; }

        public string Minor { get; set; }

        public override string ToString()
        {
            return $"{Major}.{Minor}";
        }
    }
}
