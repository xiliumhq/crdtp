using System;
using Xilium.Crdtp.Model;
using Xilium.Crdtp.Sema.Symbols;

namespace Xilium.Crdtp
{
    public class DefaultNamingPolicy : NamingPolicy
    {
        public DefaultNamingPolicy(Context context)
            : base(context)
        { }

        public override string GetDomainName(DomainSymbol domainSymbol)
            => NamingUtilities.Capitalize(domainSymbol.Name);

        public override string GetCommandName(CommandSymbol commandSymbol)
            => NamingUtilities.Capitalize(commandSymbol.Name);

        public override string GetEventName(EventSymbol eventSymbol)
            => NamingUtilities.Capitalize(eventSymbol.Name);

        public override string GetTypeName(TypeSymbol typeSymbol)
            => NamingUtilities.Capitalize(typeSymbol.Name);

        public override string GetPropertyName(PropertySymbol propertySymbol)
            => NamingUtilities.Capitalize(propertySymbol.Name);

        public override string GetAnonymousTypeName(PropertyInfo propertyInfo)
            => propertyInfo.ContainingType.Name + propertyInfo.Name;

        public override string GetEnumerationMemberName(string protocolName)
            => NamingUtilities.KebabToPascalCase(protocolName);

        public override string GetNamespaceName(DomainInfo? domainInfo)
            => GetNamespaceName(domainInfo?.Name);

        public override string GetNamespaceName(DomainInfo? domainInfo, TypeInfo typeInfo)
            => GetNamespaceName(domainInfo);

        public override string GetNamespaceName(string? @namespace)
        {
            if (@namespace == null) return Context.Options.Namespace;
            else return Context.Options.Namespace + "." + @namespace;
        }

        public override string GetDomainApiTypeName(DomainInfo domainInfo)
            => domainInfo.Name + Context.Options.DomainApiSuffix;

        public override string GetOutputItemPath(DomainInfo? domainInfo, TypeInfo typeInfo)
        {
            if (domainInfo != null)
            {
                return domainInfo.Name + "/" + typeInfo.Name + ".g.cs";
            }
            else
            {
                return typeInfo.Name + ".g.cs";
            }
        }
    }
}
