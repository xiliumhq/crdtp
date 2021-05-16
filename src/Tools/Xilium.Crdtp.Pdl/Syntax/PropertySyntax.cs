using System;
using System.Collections.Generic;

namespace Xilium.Crdtp.Pdl.Syntax
{
    public class PropertySyntax : MemberSyntax
    {
        public string Name { get; set; }
        public string Type { get; set; }
        [Obsolete]
        public TypeKind TypeKind { get; set; }
        public bool IsArray { get; set; }
        public bool IsOptional { get; set; }
        public ICollection<string> Enum { get; } = new List<string>();
    }
}
