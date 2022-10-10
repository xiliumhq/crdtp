using System;
using System.Collections.Generic;

namespace Xilium.Crdtp.Pdl.Syntax
{
    public class PropertySyntax : MemberSyntax
    {
        [Obsolete("Used only by PDL parser.")]
        public PropertySyntax() { }

        public PropertySyntax(
            // MemberSyntax
            ICollection<string> description,
            bool isDeprecated,
            bool isExperimental,
            // ProperySyntax
            string name,
            string type,
            TypeKind typeKind,
            bool isArray,
            bool isOptional,
            ICollection<string> enumMembers
            ) : base(description, isDeprecated, isExperimental)
        {
            Name = name;
            Type = type;
            TypeKind = typeKind;
            IsArray = isArray;
            IsOptional = isOptional;
            Enum = enumMembers;
        }

        public string Name { get; set; }
        public string Type { get; set; }
        [Obsolete]
        public TypeKind TypeKind { get; set; }
        public bool IsArray { get; set; }
        public bool IsOptional { get; set; }
        public ICollection<string> Enum { get; } = new List<string>();
    }
}
