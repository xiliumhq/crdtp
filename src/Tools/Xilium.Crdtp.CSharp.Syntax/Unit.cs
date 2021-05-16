namespace Xilium.Chromium.DevTools.Syntax
{

    using System.Collections.Generic;

    public sealed class Unit : SyntaxObject
    {

        public Unit(
                IEnumerable<SyntaxObject> members
                )
        {
            Members = members.AsSyntaxList();
        }

        public IEnumerable<SyntaxObject> Members { get; private set; }
    }
}
