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
    public sealed class AliasTypeInfo : TypeInfo
    {
        private readonly AliasTypeSymbol _aliasTypeSymbol;
        private readonly TypeInfo _underlyingTypeInfo;

        public AliasTypeInfo(Context context, AliasTypeSymbol aliasTypeSymbol)
            : base(context)
        {
            _aliasTypeSymbol = aliasTypeSymbol;
            _underlyingTypeInfo = context.GetTypeInfo(aliasTypeSymbol.Extends);
        }

        public override DomainInfo? Domain => GetDomainInfoOrDefault(_aliasTypeSymbol);

        public override string Name => Context.NamingPolicy.GetTypeName(_aliasTypeSymbol);

        public override string ProtocolName => _aliasTypeSymbol.Name;

        public override bool IsExperimental => _aliasTypeSymbol.IsExperimental;

        public override bool IsDeprecated => _aliasTypeSymbol.IsDeprecated;

        public override ImmutableArray<string> Description => _aliasTypeSymbol.Description;

        public TypeInfo UnderlyingType => _underlyingTypeInfo;

        public override bool IsValueType
        {
            get
            {
                if (_underlyingTypeInfo is ArrayTypeInfo)
                {
                    return false;
                }
                else if (_underlyingTypeInfo is DictionaryTypeInfo)
                {
                    return false;
                }
                else
                {
                    // Treat any alias for primitive type as value-type (e.g. including string)
                    Check.That(_underlyingTypeInfo is IntrinsicTypeInfo);
                    return true;
                }
            }
        }

        public override bool Mark(SymbolInfoFlags flags)
        {
            var marked = base.Mark(flags);
            if (marked)
            {
                marked |= UnderlyingType.Mark(flags);
            }
            return marked;
        }

        internal Emitter? GetEmitter()
        {
            if (_underlyingTypeInfo is ArrayTypeInfo)
            {
                return new AliasOfArrayTypeEmitter(Context, this);
            }
            else if (_underlyingTypeInfo is DictionaryTypeInfo)
            {
                return new AliasOfDictionaryTypeEmitter(Context, this);
            }
            else if (_underlyingTypeInfo.IsValueType)
            {
                return new PrimitiveAliasTypeEmitter(Context, this);
            }
            else
            {
                return new PrimitiveAliasTypeEmitter(Context, this);
            }
        }

        internal StjConverterEmitter? GetStjConverterEmitter()
        {
            // TODO: Converter for List<T> probably is not needed?
            if (_underlyingTypeInfo is ArrayTypeInfo)
            {
                // TODO: For special and already known types we might not emit special
                // converter, and might need just register them.
                // AliasOfArray
                return null;
            }
            else if (_underlyingTypeInfo is DictionaryTypeInfo)
            {
                // AliasOfDictionary
                return null;
            }
            else return new StjAliasConverterEmitter(Context, this);
        }
    }
}
