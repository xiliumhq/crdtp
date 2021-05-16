using System;
using System.Collections.Generic;

namespace Xilium.Crdtp.Pdl.Syntax
{
    // TODO: actually should be NamedMemberSyntax, but need care about NamedMemberWithParametersSyntax
    public class TypeSyntax : MemberSyntax
    {
        public string Name { get; set; }

        public string Extends { get; set; }

        public bool IsArray { get; set; }

        public ICollection<string> Enum { get; } = new List<string>();
        public ICollection<PropertySyntax> Properties { get; } = new List<PropertySyntax>();

        public bool IsEnum => Enum.Count > 0;
        public bool IsObject => Properties.Count > 0;
        public bool IsAlias => !IsObject && !IsEnum;
    }
}
