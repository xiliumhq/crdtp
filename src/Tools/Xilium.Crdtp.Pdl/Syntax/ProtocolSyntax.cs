using System.Collections.Generic;

namespace Xilium.Crdtp.Pdl.Syntax
{
    public sealed class ProtocolSyntax
    {
        public VersionSyntax Version { get; } = new();

        public ICollection<DomainSyntax> Domains { get; } = new List<DomainSyntax>();
    }
}
