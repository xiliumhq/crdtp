using System;
using System.Collections.Generic;

namespace Xilium.Crdtp.Pdl.Syntax
{
    public abstract class NamedMemberWithParametersSyntax : MemberSyntax
    {
        // TODO: [Obsolete("Used only by PDL parser.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected NamedMemberWithParametersSyntax() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

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
