using System.Diagnostics.CodeAnalysis;
using Xilium.Crdtp.Sema.Symbols;

namespace Xilium.Crdtp.Sema
{
    internal sealed class HidingScope : Scope
    {
        private readonly Scope _scope;
        private readonly Scope _hidden;

        public HidingScope(Scope hidden, Scope scope)
        {
            _hidden = hidden;
            _scope = scope;
        }

        public override bool TryBind(string name, [NotNullWhen(true)] out Symbol? result)
        {
            if (_scope.TryBind(name, out result))
            {
                return true;
            }

            if (_hidden.TryBind(name, out result))
            {
                return true;
            }

            return false;
        }

        public override bool TryBind<TSymbol>(string name, [NotNullWhen(true)] out TSymbol? result)
            where TSymbol : class
        {
            if (_scope.TryBind(name, out result))
            {
                return true;
            }

            if (_hidden.TryBind(name, out result))
            {
                return true;
            }

            result = default;
            return false;
        }
    }
}
