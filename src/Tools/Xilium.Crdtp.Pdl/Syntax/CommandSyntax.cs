using System;
using System.Collections.Generic;

namespace Xilium.Crdtp.Pdl.Syntax
{
    public sealed class CommandSyntax : NamedMemberWithParametersSyntax
    {
        [Obsolete("Used only by PDL parser.")]
        public CommandSyntax() { }

        public CommandSyntax(
            // MemberSyntax
            ICollection<string> description,
            bool isDeprecated,
            bool isExperimental,

            // NamedMemberWithParametersSyntax
            string name,
            ICollection<PropertySyntax> parameters,

            // CommandSyntax
            ICollection<PropertySyntax> returns,
            string? redirect,
            ICollection<string> redirectDescription)
            : base(description, isDeprecated, isExperimental,
                  name, parameters)
        {
            Returns = returns;
            Redirect = redirect;
            RedirectDescription = redirectDescription;
        }

        public ICollection<PropertySyntax> Returns { get; } = new List<PropertySyntax>();

        /// <summary>
        /// Contains domain name. E.g. typical description e.g. canonical case is:
        /// <code>
        /// # Hides any highlight.
        /// command hideHighlight
        ///   # Use 'Overlay.hideHighlight' instead
        ///   redirect Overlay
        /// </code>
        /// </summary>
        public string? Redirect { get; set; }
        public ICollection<string> RedirectDescription { get; set; } = new List<string>();
    }
}
