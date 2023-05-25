namespace Xilium.Chromium.DevTools.Syntax
{
    using System.Collections.Generic;

    public sealed class Namespace : SyntaxObject
    {
        public Namespace(
                string name,
                IEnumerable<SyntaxObject> members,
                XmlDocumentation? xmlDocumentation = null)
        {
            Name = name;
            Members = members.AsSyntaxList();
            XmlDocumentation = xmlDocumentation;
        }

        public string Name { get; private set; }

        public IEnumerable<SyntaxObject> Members { get; private set; }

        public XmlDocumentation? XmlDocumentation { get; private set; }
    }
}
