using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xilium.Crdtp.Emitters;
using Xilium.Crdtp.Sema.Symbols;

namespace Xilium.Crdtp.Model
{
    public sealed class EnumTypeInfo : TypeInfo
    {
        private readonly EnumTypeSymbol _enumTypeSymbol;
        private readonly ImmutableArray<EnumMemberInfo> _members;

        public EnumTypeInfo(Context context, EnumTypeSymbol enumTypeSymbol)
            : base(context)
        {
            _enumTypeSymbol = enumTypeSymbol;
            Check.That(_enumTypeSymbol.Extends == _enumTypeSymbol.Compilation!.Intrinsics.String);

            _members = _enumTypeSymbol.Members
                .Select(x => new EnumMemberInfo(Context.NamingPolicy.GetEnumerationMemberName(x), x))
                .ToImmutableArray();
        }

        public override DomainInfo? Domain => GetDomainInfoOrDefault(_enumTypeSymbol);

        public override string Name
        {
            get
            {
                if (_enumTypeSymbol.Name == null) // is anonymous
                {
                    var parentSymbol = _enumTypeSymbol.ContainingSymbol;
                    Check.That(parentSymbol != null);

                    var parentSymbolInfo = Context.GetSymbolInfo(parentSymbol);
                    if (parentSymbolInfo is PropertyInfo propertyInfo)
                    {
                        return Context.NamingPolicy.GetAnonymousTypeName(propertyInfo);
                    }
                    else throw Error.InvalidOperation();
                }
                else
                {
                    return Context.NamingPolicy.GetTypeName(_enumTypeSymbol);
                }
            }
        }

        public override string ProtocolName => _enumTypeSymbol.Name;

        public override bool IsExperimental => _enumTypeSymbol.IsExperimental;

        public override bool IsDeprecated => _enumTypeSymbol.IsDeprecated;

        public override ImmutableArray<string> Description => _enumTypeSymbol.Description;

        public ImmutableArray<EnumMemberInfo> Members => _members;

        public override bool IsValueType => true;

        internal Emitter? GetEmitter()
        {
            return new EnumTypeEmitter(Context, this);
        }

        internal StjConverterEmitter? GetStjConverterEmitter()
        {
            return new StjEnumConverterEmitter(Context, this);
        }
    }
}
