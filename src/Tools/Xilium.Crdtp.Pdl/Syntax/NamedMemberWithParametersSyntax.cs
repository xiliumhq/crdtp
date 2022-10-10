using System;
using System.Collections.Generic;

namespace Xilium.Crdtp.Pdl.Syntax
{
    public abstract class NamedMemberWithParametersSyntax : MemberSyntax
    {
        [Obsolete("Used only by PDL parser.")]
        protected NamedMemberWithParametersSyntax() { }

        protected NamedMemberWithParametersSyntax(
            // MemberSyntax
            ICollection<string> description,
            bool isDeprecated,
            bool isExperimental,

            // NamedMemberWithParametersSyntax
            string name,
            ICollection<PropertySyntax> parameters)
            : base(description, isDeprecated, isExperimental)
        {
            Name = name;
            Parameters = parameters;
        }

        public string Name { get; set; }
        public ICollection<PropertySyntax> Parameters { get; } = new List<PropertySyntax>();
    }
}
