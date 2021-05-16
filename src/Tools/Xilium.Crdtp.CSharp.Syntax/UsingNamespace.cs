namespace Xilium.Chromium.DevTools.Syntax
{

    public sealed class UsingNamespace : SyntaxObject
    {

        public UsingNamespace(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}
