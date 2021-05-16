namespace Xilium.Chromium.DevTools.Syntax
{

    using System.Collections.Generic;

    public sealed class IfStatement : SyntaxObject
    {

        public IfStatement(
                string condition,
                IEnumerable<SyntaxObject> members
                )
        {
            Condition = condition;
            Members = members ?? System.Array.Empty<SyntaxObject>();
        }

        public string Condition { get; private set; }

        public IEnumerable<SyntaxObject> Members { get; private set; }
    }
}
