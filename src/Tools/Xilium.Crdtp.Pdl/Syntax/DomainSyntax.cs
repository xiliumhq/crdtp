using System.Collections.Generic;

namespace Xilium.Crdtp.Pdl.Syntax
{
    public sealed class DomainSyntax : MemberSyntax
    {
        public string Name { get; set; }
        public ICollection<string> Depends { get; } = new HashSet<string>();
        public ICollection<TypeSyntax> Types { get; } = new List<TypeSyntax>();
        public ICollection<CommandSyntax> Commands { get; } = new List<CommandSyntax>();
        public ICollection<EventSyntax> Events { get; } = new List<EventSyntax>();
    }
}
