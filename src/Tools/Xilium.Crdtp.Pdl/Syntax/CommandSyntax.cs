using System.Collections.Generic;

namespace Xilium.Crdtp.Pdl.Syntax
{
    public sealed class CommandSyntax : NamedMemberWithParametersSyntax
    {
        public ICollection<PropertySyntax> Returns { get; } = new List<PropertySyntax>();
        public string Redirect { get; set; }
        public ICollection<string> RedirectDescription { get; set; } = new List<string>();
    }
}
