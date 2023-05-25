using System;
using System.Collections.Generic;

namespace Xilium.Crdtp.Pdl.Syntax
{
    public class PropertySyntax : MemberSyntax
    {
        // TODO: [Obsolete("Used only by PDL parser.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public PropertySyntax() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

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
#pragma warning disable CS0612 // Type or member is obsolete
            TypeKind = typeKind;
#pragma warning restore CS0612 // Type or member is obsolete
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
