using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Sx = Xilium.Crdtp.Pdl.Syntax;

namespace Xilium.Crdtp.Sema.Symbols.Source
{
    internal sealed class SourceDomainSymbol : DomainSymbol
    {
        private readonly SourceProtocolSymbol _containingSymbol;
        private readonly Sx.DomainSyntax _domainSyntax;
        private readonly Scope _scope;
        private readonly ImmutableArray<string> _description;

        private ImmutableArray<DomainSymbol>? _depends;
        private ImmutableArray<TypeSymbol>? _types;
        private ImmutableArray<CommandSymbol>? _commands;
        private ImmutableArray<EventSymbol>? _events;

        public SourceDomainSymbol(SourceProtocolSymbol containingSymbol, Sx.DomainSyntax domainSyntax, Scope scope)
        {
            _containingSymbol = containingSymbol;
            _domainSyntax = domainSyntax;
            _scope = scope;
            _description = _domainSyntax.Description.ToImmutableArray();
        }

        public override Symbol? ContainingSymbol => _containingSymbol;

        public override string Name => _domainSyntax.Name;

        public override ImmutableArray<DomainSymbol> Depends => _depends ?? GetDepends();

        public override ImmutableArray<TypeSymbol> Types => _types!.Value;

        public override ImmutableArray<CommandSymbol> Commands => _commands!.Value;

        public override ImmutableArray<EventSymbol> Events => _events!.Value;

        public override bool IsDeprecated => _domainSyntax.IsDeprecated;

        public override bool IsExperimental => _domainSyntax.IsExperimental;

        public override ImmutableArray<string> Description => _description;

        internal void Declare()
        {
            var declaringScope = new DeclaringScope();
            var scope = _scope.HideWith(declaringScope);

            // Types
            var types = new List<TypeSymbol>();
            foreach (var typeSyntax in _domainSyntax.Types)
            {
                if (typeSyntax.IsAlias)
                {
                    var typeSymbol = new SourceAliasTypeSymbol(this, typeSyntax, scope);
                    types.Add(typeSymbol);
                    declaringScope.Add(typeSymbol.Name, typeSymbol);
                    typeSymbol.Declare();
                }
                else if (typeSyntax.IsEnum)
                {
                    var typeSymbol = new SourceEnumTypeSymbol(this, typeSyntax, scope);
                    types.Add(typeSymbol);
                    declaringScope.Add(typeSymbol.Name, typeSymbol);
                    typeSymbol.Declare();
                }
                else if (typeSyntax.IsObject)
                {
                    var typeSymbol = new SourceObjectTypeSymbol(this, typeSyntax, scope);
                    types.Add(typeSymbol);
                    declaringScope.Add(typeSymbol.Name, typeSymbol);
                    typeSymbol.Declare();
                }
                else throw Error.InvalidOperation("Unknown type declaration.");
            }
            _types = types.ToImmutableArray();

            // Commands
            var commands = new List<CommandSymbol>();
            foreach (var commandSyntax in _domainSyntax.Commands)
            {
                var commandSymbol = new SourceCommandSymbol(this, commandSyntax, scope);
                commands.Add(commandSymbol);
                declaringScope.Add(commandSymbol.Name, commandSymbol);
                commandSymbol.Declare();
            }
            _commands = commands.ToImmutableArray();

            // Events
            var events = new List<EventSymbol>();
            foreach (var eventSyntax in _domainSyntax.Events)
            {
                var eventSymbol = new SourceEventSymbol(this, eventSyntax, scope);
                events.Add(eventSymbol);
                declaringScope.Add(eventSymbol.Name, eventSymbol);
                eventSymbol.Declare();
            }
            _events = events.ToImmutableArray();
        }

        private ImmutableArray<DomainSymbol> GetDepends()
        {
            var depends = new List<DomainSymbol>();
            foreach (var domainName in _domainSyntax.Depends)
            {
                var domainSymbol = _scope.Bind<DomainSymbol>(domainName);
                depends.Add(domainSymbol);
            }

            _depends = depends.ToImmutableArray();
            return _depends.Value;
        }
    }
}
