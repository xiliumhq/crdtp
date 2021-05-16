namespace Xilium.Chromium.DevTools.Syntax
{

    public sealed class EmptyLine : SyntaxObject
    {
        public EmptyLine(bool endOfLine = true)
        {
            Eol = endOfLine;
        }

        public bool Eol { get; private set; }
    }
}
