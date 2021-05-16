using System.Diagnostics.CodeAnalysis;
using Xilium.Crdtp.Sema.Symbols;

namespace Xilium.Crdtp.Sema
{
    public abstract class Scope
    {
        public abstract bool TryBind(string name, [NotNullWhen(true)] out Symbol? result);
        public abstract bool TryBind<TSymbol>(string name, [NotNullWhen(true)] out TSymbol? result)
            where TSymbol : Symbol;

        public Symbol Bind(string name)
        {
            if (TryBind(name, out var result)) return result;
            throw Error.InvalidOperation("Unable to bind symbol \"{0}\".", name);
        }

        public TSymbol Bind<TSymbol>(string name) where TSymbol : Symbol
        {
            if (TryBind<TSymbol>(name, out var result)) return result;
            throw Error.InvalidOperation("Unable to bind symbol \"{0}\".", name);
        }

        //public Scope<TSymbol> UnionWith(Scope<TSymbol>? scope)
        //{
        //    if (scope == null) return this;
        //    return new UnionScope<TSymbol>(this, scope);
        //}

        public Scope HideWith(Scope? scope)
        {
            if (scope == null) return this;
            return new HidingScope(this, scope);
        }
    }
}
