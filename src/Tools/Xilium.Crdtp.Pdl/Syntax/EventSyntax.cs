using System.Collections.Generic;
using System;

namespace Xilium.Crdtp.Pdl.Syntax
{
    public sealed class EventSyntax : NamedMemberWithParametersSyntax
    {
        // TODO: [Obsolete("Used only by PDL parser.")]
        public EventSyntax() { }

        public EventSyntax(
            // MemberSyntax
            ICollection<string> description,
            bool isDeprecated,
            bool isExperimental,

            // NamedMemberWithParametersSyntax
            string name,
            ICollection<PropertySyntax> parameters)
            : base(description, isDeprecated, isExperimental,
                  name, parameters)
        {
        }
    }
}
