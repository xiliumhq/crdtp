namespace Xilium.Chromium.DevTools.Syntax
{

    public sealed class Comment : SyntaxObject
    {

        public Comment(string value)
        {
            Value = value;
        }

        public string Value { get; private set; }
    }
}
