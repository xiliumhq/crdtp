using System.Collections.Immutable;
using System.Linq;
using Sx = Xilium.Crdtp.Pdl.Syntax;

namespace Xilium.Crdtp.Sema.Symbols.Source
{
    internal class SourcePropertySymbol : PropertySymbol
    {
        private readonly Symbol _containingSymbol;
        private readonly Sx.PropertySyntax _propertySyntax;
        private readonly Scope _scope;
        private readonly ImmutableArray<string> _description;

        private QualifiedType? _type;

        public SourcePropertySymbol(Symbol containingSymbol, Sx.PropertySyntax propertySyntax, Scope scope)
        {
            _containingSymbol = containingSymbol;
            _propertySyntax = propertySyntax;
            _scope = scope;
            _description = _propertySyntax.Description.ToImmutableArray();
        }

        public override Symbol? ContainingSymbol => _containingSymbol;

        public override string Name => _propertySyntax.Name;

        public override QualifiedType Type => _type ?? GetPropertyType();

        public override bool IsDeprecated => _propertySyntax.IsDeprecated;

        public override bool IsExperimental => _propertySyntax.IsExperimental;

        public override ImmutableArray<string> Description => _description;

        internal void Declare()
        {
        }

        private QualifiedType GetPropertyType()
        {
            // Resolve Type Name
            var compilation = Compilation;
            Check.That(compilation != null);

            TypeSymbol? typeSymbol = null;
            if (_propertySyntax.Enum.Count > 0)
            {
                Check.That(_propertySyntax.IsArray == false);
                typeSymbol = new SourceAnonymousEnumTypeSymbol(this, _propertySyntax, _scope);
            }
            else
            {
                var name = _propertySyntax.Type;
                if (!name.Contains('.'))
                {
                    typeSymbol = _scope.Bind<TypeSymbol>(name);
                }
                else
                {
                    var parts = name.Split('.');
                    Check.That(parts.Length == 2, "Invalid multi-part identifier.");

                    var domainName = parts[0];
                    var typeName = parts[1];

                    var domainSymbol = _scope.Bind<DomainSymbol>(domainName);
                    typeSymbol = domainSymbol.Types.Where(x => x.Name == typeName).FirstOrDefault();
                }
            }

            Check.That(typeSymbol != null);

            if (_propertySyntax.IsArray)
            {
                typeSymbol = Compilation!.CreateArrayType(typeSymbol);
            }

            var qualifiers = _propertySyntax.IsOptional ? TypeQualifiers.Optional : TypeQualifiers.None;
            var qualifiedType = new QualifiedType(typeSymbol, qualifiers);

            _type = qualifiedType;
            return qualifiedType;
        }
    }
}
