namespace Xilium.Chromium.DevTools.Syntax
{

    public sealed class Raw : SyntaxObject
    {

        public Raw(string value)
        {
            Value = value;
        }

        public string Value { get; private set; }
    }
}
