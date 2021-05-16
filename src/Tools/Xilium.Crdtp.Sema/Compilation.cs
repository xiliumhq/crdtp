using System.Collections.Generic;
using System.Linq;
using Sx = Xilium.Crdtp.Pdl.Syntax;
using Sy = Xilium.Crdtp.Sema.Symbols;

namespace Xilium.Crdtp.Sema
{
    public sealed class Compilation
    {
        public static Compilation Create(IEnumerable<Sx.ProtocolSyntax> syntaxTrees)
        {
            var syntaxTreeList = syntaxTrees.ToList();
            Check.That(syntaxTreeList.Count > 0);
            return new Compilation(syntaxTreeList);
        }

        private readonly List<Sx.ProtocolSyntax> _syntaxTrees;
        private readonly DeclaringScope _rootScope;
        private readonly Intrinsics _intrinsics;
        private Sy.ProtocolSymbol? _rootSymbol;

        private Compilation(List<Sx.ProtocolSyntax> syntaxTrees)
        {
            _syntaxTrees = syntaxTrees;

            _rootScope = new DeclaringScope();
            _intrinsics = new Intrinsics(_rootScope);
        }

        public Intrinsics Intrinsics => _intrinsics;

        public IEnumerable<Sx.ProtocolSyntax> GetSyntaxTrees()
            => _syntaxTrees;

        public Sy.ProtocolSymbol Protocol => GetRootSymbol();

        private Sy.ProtocolSymbol GetRootSymbol()
        {
            if (_rootSymbol != null) return _rootSymbol;
            var rootSymbol = new Sy.Source.SourceProtocolSymbol(this, _syntaxTrees);
            rootSymbol.Declare();
            _rootSymbol = rootSymbol;
            return rootSymbol;
        }

        internal Scope GetRootScope() => _rootScope;

        private readonly Dictionary<Sy.TypeSymbol, Sy.ArrayTypeSymbol> _arrayTypes = new Dictionary<Sy.TypeSymbol, Sy.ArrayTypeSymbol>();

        internal Sy.ArrayTypeSymbol CreateArrayType(Sy.TypeSymbol elementType)
        {
            if (!_arrayTypes.TryGetValue(elementType, out var result))
            {
                result = new Sy.ArrayTypeSymbol(elementType);
                _arrayTypes.Add(elementType, result);
            }
            return result;
        }
    }
}
