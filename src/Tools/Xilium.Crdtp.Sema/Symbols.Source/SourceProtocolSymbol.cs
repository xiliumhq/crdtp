using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Sx = Xilium.Crdtp.Pdl.Syntax;

namespace Xilium.Crdtp.Sema.Symbols.Source
{
    internal sealed class SourceProtocolSymbol : ProtocolSymbol
    {
        private readonly Compilation _compilation;
        private readonly List<Sx.ProtocolSyntax> _syntaxTrees;

        private ProtocolVersion? _version;
        private ImmutableArray<DomainSymbol>? _domains;

        public SourceProtocolSymbol(Compilation compilation, List<Sx.ProtocolSyntax> syntaxTrees)
        {
            _compilation = compilation;
            _syntaxTrees = syntaxTrees;
        }

        public override Compilation? Compilation => _compilation;

        public override ProtocolVersion Version => _version!;

        public override ImmutableArray<DomainSymbol> Domains => _domains!.Value;

        internal void Declare()
        {
            var rootScope = _compilation.GetRootScope();

            ProtocolVersion? version = null;
            var domains = new List<DomainSymbol>();
            var declaringScope = new DeclaringScope();
            var scope = rootScope.HideWith(declaringScope);

            foreach (var protocolSyntax in _syntaxTrees)
            {
                if (version == null)
                {
                    version = new ProtocolVersion(protocolSyntax.Version.Major, protocolSyntax.Version.Minor);
                }
                else
                {
                    // TODO: emit warning if version mismatch
                }

                foreach (var domainSyntax in protocolSyntax.Domains)
                {
                    var domainSymbol = new SourceDomainSymbol(this, domainSyntax, scope);
                    domains.Add(domainSymbol);
                    declaringScope.Add(domainSymbol.Name, domainSymbol);
                    domainSymbol.Declare();
                }
            }

            _version = version;
            _domains = domains.ToImmutableArray();
        }
    }
}
