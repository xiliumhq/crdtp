using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xilium.Crdtp.Emitters
{
    internal abstract class Emitter
    {
        private readonly Context _context;
        private readonly WellKnownTypes _wellKnownTypes;

        protected Emitter(Context context)
        {
            _context = context;
            _wellKnownTypes = new WellKnownTypes(context);
        }

        protected Context Context => _context;

        protected WellKnownTypes WellKnownTypes => _wellKnownTypes;

        public abstract void Emit();
    }
}
