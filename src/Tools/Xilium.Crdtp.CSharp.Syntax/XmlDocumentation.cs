using System.Collections.Generic;

namespace Xilium.Chromium.DevTools.Syntax
{

    public sealed class XmlDocumentation
    {
        public XmlDocumentation(string? summary,
            IEnumerable<XmlDocParam>? paramList = null)
        {
            Summary = summary;
            ParamList = paramList.AsSyntaxList();
        }

        public string? Summary { get; private set; }
        public IEnumerable<XmlDocParam> ParamList { get; private set; }
    }
}
