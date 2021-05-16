using System.Collections.Generic;

namespace Xilium.Crdtp.Pdl.Syntax
{
    public abstract class NamedMemberWithParametersSyntax : MemberSyntax
    {
        public string Name { get; set; }
        public ICollection<PropertySyntax> Parameters { get; } = new List<PropertySyntax>();
    }
}
