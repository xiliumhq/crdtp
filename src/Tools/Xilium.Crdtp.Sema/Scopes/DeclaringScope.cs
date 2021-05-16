using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xilium.Crdtp.Sema.Symbols;

namespace Xilium.Crdtp.Sema
{
    internal sealed class DeclaringScope : Scope
    {
        private readonly Dictionary<string, Symbol> _symbols;

        public DeclaringScope()
        {
            _symbols = new Dictionary<string, Symbol>();
        }

        public void Add(string name, Symbol symbol) // TODO: should be something like Add<TSymbol>(Declaration decl, ...)
        {
            _symbols.Add(name, symbol);
        }

        public override bool TryBind(string name, [NotNullWhen(true)] out Symbol? result)
        {
            return _symbols.TryGetValue(name, out result);
        }

        public override bool TryBind<TSymbol>(string name, [NotNullWhen(true)] out TSymbol? result)
            where TSymbol : class
        {
            if (_symbols.TryGetValue(name, out var tempSymbol))
            {
                if (tempSymbol is TSymbol x)
                {
                    result = x;
                    return true;
                }
            }

            result = default;
            return false;
        }
    }
}
