using System;
using System.Collections.Generic;

namespace Xilium.Crdtp.Pdl.Syntax
{
    // TODO: actually should be NamedMemberSyntax, but need care about NamedMemberWithParametersSyntax
    public class TypeSyntax : MemberSyntax
    {
        [Obsolete("Used only by PDL parser.")]
        public TypeSyntax() { }

        public TypeSyntax(
            // MemberSyntax
            ICollection<string> description,
            bool isDeprecated,
            bool isExperimental,

            // TypeSyntax
            string name,
            string extends,
            bool isArray,
            ICollection<string> enumValues,
            ICollection<PropertySyntax> properties)
            : base(description, isDeprecated, isExperimental)
        {
            Name = name;
            Extends = extends;
            IsArray = isArray;
            Enum = enumValues;
            Properties = properties;
        }

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
