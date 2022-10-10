using System;
using System.Collections.Generic;

namespace Xilium.Crdtp.Pdl.Syntax
{
    public sealed class ProtocolSyntax
    {
        [Obsolete("Used by PDL parser, but it should not be used in such way.")]
        public ProtocolSyntax() { }

        public ProtocolSyntax(VersionSyntax version,
            List<DomainSyntax> domains)
        {
            Version = version;
            Domains = domains;
        }

        public VersionSyntax Version { get; } = new();

        public ICollection<DomainSyntax> Domains { get; } = new List<DomainSyntax>();
    }
}
