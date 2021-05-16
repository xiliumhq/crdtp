using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xilium.Crdtp.Pdl.Syntax
{
    public sealed class VersionSyntax
    {
        public string Major { get; set; }

        public string Minor { get; set; }

        public override string ToString()
        {
            return $"{Major}.{Minor}";
        }
    }
}
