using System.Collections.Generic;
using System.Linq;
using Xilium.Crdtp.Model;
using CS = Xilium.Chromium.DevTools.Syntax;

namespace Xilium.Crdtp.Emitters
{
    internal sealed class PrimitiveAliasTypeEmitter : AliasTypeEmitter
    {
        public PrimitiveAliasTypeEmitter(Context context, AliasTypeInfo typeInfo)
            : base(context, typeInfo)
        { }
    }
}
