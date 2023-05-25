using System.Collections.Generic;

namespace Xilium.Crdtp.Pdl.Syntax
{
    public abstract class MemberSyntax
    {
        // TODO: [Obsolete("Used only by PDL parser.")]
        protected MemberSyntax() { }

        protected MemberSyntax(ICollection<string> description,
            bool isDeprecated,
            bool isExperimental)
        {
            Description = description;
            IsDeprecated = isDeprecated;
            IsExperimental = isExperimental;
        }

        public ICollection<string> Description { get; set; } = new List<string>();
        public bool IsDeprecated { get; set; }
        public bool IsExperimental { get; set; }
    }
}
