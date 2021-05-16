namespace Xilium.Chromium.DevTools.Syntax
{

    using System.Collections.Generic;

    public sealed class Subconstructor : SyntaxObject
    {

        public Subconstructor(AccessKeywords keyword, IEnumerable<Parameter> parameters)
        {
            Keyword = keyword;
            Parameters = parameters.AsSyntaxList();
        }

        public AccessKeywords Keyword { get; private set; }

        public IEnumerable<Parameter> Parameters { get; private set; }
    }
}
