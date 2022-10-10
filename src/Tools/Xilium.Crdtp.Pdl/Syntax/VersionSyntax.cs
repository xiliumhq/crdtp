using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xilium.Crdtp.Pdl.Syntax
{
    public sealed class VersionSyntax
    {
        [Obsolete("Used by PDL parser, but it should not be used in such way.")]
        public VersionSyntax() { }

        public VersionSyntax(string major, string minor)
        {
            Major = major;
            Minor = minor;
        }

        public string Major { get; set; }

        public string Minor { get; set; }

        public override string ToString()
        {
            return $"{Major}.{Minor}";
        }
    }
}
