namespace Xilium.Chromium.DevTools.Syntax
{

    using System.Collections.Generic;

    public sealed class CodeBlock : SyntaxObject
    {

        public CodeBlock(
                IEnumerable<SyntaxObject> members, bool indent = false, bool terminateStatement = false
                )
        {
            Members = members.AsSyntaxList();
            Indent = indent;
            TerminateStatement = terminateStatement;
        }

        public IEnumerable<SyntaxObject> Members { get; private set; }

        public bool Indent { get; }
        public bool TerminateStatement { get; }
    }
}
