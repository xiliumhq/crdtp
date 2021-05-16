using System.Collections.Generic;

namespace Xilium.Crdtp.Pdl.Syntax
{
    public abstract class MemberSyntax
    {
        public ICollection<string> Description { get; set; } = new List<string>();
        public bool IsDeprecated { get; set; }
        public bool IsExperimental { get; set; }
    }
}
