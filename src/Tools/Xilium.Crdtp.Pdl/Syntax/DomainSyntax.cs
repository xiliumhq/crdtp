using System;
using System.Collections.Generic;

namespace Xilium.Crdtp.Pdl.Syntax
{
    public sealed class DomainSyntax : MemberSyntax
    {
        [Obsolete("Used only by PDL parser.")]
        public DomainSyntax() { }

        public DomainSyntax(
            // member syntax
            ICollection<string> description,
            bool isDeprecated,
            bool isExperimental,

            // domain props
            string name,
            ICollection<string> depends,
            ICollection<TypeSyntax> types,
            ICollection<CommandSyntax> commands,
            ICollection<EventSyntax> events)
            : base(description, isDeprecated, isExperimental)
        {
            Name = name;
            Depends = depends;
            Types = types;
            Commands = commands;
            Events = events;
        }

        public string Name { get; set; }
        public ICollection<string> Depends { get; } = new HashSet<string>();
        public ICollection<TypeSyntax> Types { get; } = new List<TypeSyntax>();
        public ICollection<CommandSyntax> Commands { get; } = new List<CommandSyntax>();
        public ICollection<EventSyntax> Events { get; } = new List<EventSyntax>();
    }
}
