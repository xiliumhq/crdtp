using System.Collections.Immutable;
using Xilium.Crdtp.Sema.Symbols;

namespace Xilium.Crdtp.Model
{
    public abstract class MemberInfo : SymbolInfo
    {
        protected internal MemberInfo(Context context)
            : base(context)
        { }

        public abstract string Name { get; }

        public abstract string ProtocolName { get; }

        public abstract DomainInfo? Domain { get; }

        public abstract bool IsExperimental { get; }
        public abstract bool IsDeprecated { get; }
        public abstract ImmutableArray<string> Description { get; }

        protected DomainInfo? GetDomainInfoOrDefault(Symbol symbol)
        {
            var domainSymbol = symbol.DomainSymbol;
            if (domainSymbol != null)
            {
                return Context.GetDomainInfo(domainSymbol);
            }
            return default;
        }

        public virtual string GetTypeInfoPropertyName()
        {
            return Domain?.Name + "_" + GetOwnTypeInfoPropertyName();
        }

        public virtual string GetOwnTypeInfoPropertyName() => Name;
        public virtual bool UseInSerializationContext() => true;
    }
}
