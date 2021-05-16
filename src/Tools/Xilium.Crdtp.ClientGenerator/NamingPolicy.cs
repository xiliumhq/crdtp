using Xilium.Crdtp.Model;
using Xilium.Crdtp.Sema.Symbols;

namespace Xilium.Crdtp
{
    public abstract class NamingPolicy
    {
        private readonly Context _context;

        protected NamingPolicy(Context context)
        {
            _context = context;
        }

        protected Context Context => _context;


        public abstract string GetDomainName(DomainSymbol domainSymbol);
        public abstract string GetCommandName(CommandSymbol commandSymbol);
        public abstract string GetEventName(EventSymbol eventSymbol);
        public abstract string GetTypeName(TypeSymbol typeSymbol);
        public abstract string GetPropertyName(PropertySymbol propertySymbol);
        public abstract string GetAnonymousTypeName(PropertyInfo propertyInfo);
        public abstract string GetEnumerationMemberName(string protocolName);

        public abstract string GetNamespaceName(string? @namespace);
        public abstract string GetNamespaceName(DomainInfo? domainInfo);
        public abstract string GetNamespaceName(DomainInfo? domainInfo, TypeInfo typeInfo);

        public abstract string GetDomainApiTypeName(DomainInfo domainInfo);

        public abstract string GetOutputItemPath(DomainInfo? domainSymbol, TypeInfo typeInfo);
    }
}
