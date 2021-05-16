using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Xilium.Crdtp.Emitters;
using Xilium.Crdtp.Model;
using Xilium.Crdtp.Sema.Symbols;

namespace Xilium.Crdtp
{
    public sealed class Context
    {
        private readonly Dictionary<Symbol, SymbolInfo> _symbolInfos;
        private readonly HashSet<SymbolInfo> _reachableNodes;

        private readonly List<StjConverterEmitter> _stjConverterEmitters;

        // intrinsics
        private readonly AnyTypeInfo _anyTypeInfo;
        private readonly BinaryTypeInfo _binaryTypeInfo;
        private readonly BooleanTypeInfo _booleanTypeInfo;
        private readonly IntegerTypeInfo _integerTypeInfo;
        private readonly NumberTypeInfo _numberTypeInfo;
        private readonly DictionaryTypeInfo _dictionaryTypeInfo;
        private readonly StringTypeInfo _stringTypeInfo;

        private readonly UnitTypeInfo _unitTypeInfo;

        public Context()
        {
            CSharp = new CSharpCodeHelper(this);
            _symbolInfos = new();
            _reachableNodes = new();
            _stjConverterEmitters = new();

            _anyTypeInfo = new(this);
            _binaryTypeInfo = new(this);
            _booleanTypeInfo = new(this);
            _integerTypeInfo = new(this);
            _numberTypeInfo = new(this);
            _dictionaryTypeInfo = new(this);
            _stringTypeInfo = new(this);

            _unitTypeInfo = new(this);
        }

        public OutputScope OutputScope { get; init; }
        public ClientGeneratorOptions Options { get; init; }
        public DefaultNamingPolicy NamingPolicy { get; internal set; }
        internal CSharpCodeHelper CSharp { get; }

        public SymbolInfo GetSymbolInfo(Symbol symbol)
        {
            Check.That(symbol != null);

            if (!_symbolInfos.TryGetValue(symbol, out var symbolInfo))
            {
                symbolInfo = CreateSymbolInfo(symbol);
                _symbolInfos.Add(symbol, symbolInfo);
            }
            return symbolInfo;
        }

        public DomainInfo GetDomainInfo(DomainSymbol symbol)
            => (DomainInfo)GetSymbolInfo(symbol);

        public TypeInfo GetTypeInfo(TypeSymbol symbol)
            => (TypeInfo)GetSymbolInfo(symbol);

        public CommandInfo GetCommandInfo(CommandSymbol symbol)
            => (CommandInfo)GetSymbolInfo(symbol);

        public EventInfo GetEventInfo(EventSymbol symbol)
            => (EventInfo)GetSymbolInfo(symbol);

        internal void AddPropertySymbol(PropertySymbol propertySymbol, PropertyInfo propertyInfo)
        {
            _symbolInfos.Add(propertySymbol, propertyInfo);
        }

        private SymbolInfo CreateSymbolInfo(Symbol symbol)
        {
            switch (symbol)
            {
                case DomainSymbol x: return new DomainInfo(this, x);
                case CommandSymbol x: return new CommandInfo(this, x);
                case EventSymbol x: return new EventInfo(this, x);

                case TypeSymbol typeSymbol:
                    switch (typeSymbol)
                    {
                        case ObjectTypeSymbol x: return new SymbolObjectTypeInfo(this, x);
                        case AliasTypeSymbol x: return new AliasTypeInfo(this, x);
                        case EnumTypeSymbol x: return new EnumTypeInfo(this, x);
                        case ArrayTypeSymbol x: return new ArrayTypeInfo(this, x);
                        case IntrinsicTypeSymbol x:
                            return x.Kind switch
                            {
                                IntrinsicTypeKind.Any => _anyTypeInfo,
                                IntrinsicTypeKind.Binary => _binaryTypeInfo,
                                IntrinsicTypeKind.Boolean => _booleanTypeInfo,
                                IntrinsicTypeKind.Integer => _integerTypeInfo,
                                IntrinsicTypeKind.Number => _numberTypeInfo,
                                IntrinsicTypeKind.Object => _dictionaryTypeInfo,
                                IntrinsicTypeKind.String => _stringTypeInfo,
                                _ => throw Error.InvalidOperation("Invalid IntrinsicTypeKind={0}", x.Kind),
                            };

                        default: throw Error.InvalidOperation("Can't create type info.");
                    }

                default: throw Error.InvalidOperation("Can't create symbol info");
            }
        }

        internal TypeInfo CreateSyntheticTypeOrUnitType(SyntheticObjectTypeInfo.Kind kind, MemberInfo containingMember, ImmutableArray<PropertySymbol> properties)
        {
            if (properties.Length > 0)
            {
                return new SyntheticObjectTypeInfo(this, kind, containingMember, properties);
            }
            else
            {
                return _unitTypeInfo;
            }
        }

        internal void MarkReachable(SymbolInfo symbolInfo)
        {
            Check.That(symbolInfo.IsReachable);
            _reachableNodes.Add(symbolInfo);
        }

        internal IEnumerable<SymbolInfo> GetReachableNodes()
        {
            return _reachableNodes;
        }

        internal IEnumerable<TSymbolInfo> GetReachableNodes<TSymbolInfo>()
            where TSymbolInfo : SymbolInfo
        {
            return _reachableNodes.OfType<TSymbolInfo>();
        }

        internal void AddStjConverterEmitter(StjConverterEmitter emitter)
        {
            _stjConverterEmitters.Add(emitter);
        }

        internal IReadOnlyList<StjConverterEmitter> StjConverterEmitters => _stjConverterEmitters;
    }
}
