namespace Xilium.Chromium.DevTools.Syntax
{

    public sealed class XmlDocParam
    {

        public XmlDocParam(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; private set; }
        public string Description { get; private set; }
    }
}
