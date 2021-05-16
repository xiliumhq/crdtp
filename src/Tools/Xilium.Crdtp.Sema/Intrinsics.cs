using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xilium.Crdtp.Sema.Symbols;
using Xilium.Crdtp.Sema.Symbols.Source;

namespace Xilium.Crdtp.Sema
{
    public sealed class Intrinsics
    {
        public IntrinsicTypeSymbol Any { get; }
        public IntrinsicTypeSymbol Binary { get; }
        public IntrinsicTypeSymbol Boolean { get; }
        public IntrinsicTypeSymbol Integer { get; }
        public IntrinsicTypeSymbol Number { get; }
        public IntrinsicTypeSymbol Object { get; }
        public IntrinsicTypeSymbol String { get; }

        internal Intrinsics(DeclaringScope scope)
        {
            IntrinsicTypeSymbol Declare(IntrinsicTypeSymbol type)
            {
                scope.Add(type.Name, type);
                return type;
            }

            Any = Declare(new IntrinsicTypeSymbol(IntrinsicTypeKind.Any, "any"));
            Binary = Declare(new IntrinsicTypeSymbol(IntrinsicTypeKind.Binary, "binary"));
            Boolean = Declare(new IntrinsicTypeSymbol(IntrinsicTypeKind.Boolean, "boolean"));
            Integer = Declare(new IntrinsicTypeSymbol(IntrinsicTypeKind.Integer, "integer"));
            Number = Declare(new IntrinsicTypeSymbol(IntrinsicTypeKind.Number, "number"));
            Object = Declare(new IntrinsicTypeSymbol(IntrinsicTypeKind.Object, "object"));
            String = Declare(new IntrinsicTypeSymbol(IntrinsicTypeKind.String, "string"));
        }
    }
}
