namespace Xilium.Chromium.DevTools.Syntax
{
    public sealed class ArrowExpressionClause
    {
        public ArrowExpressionClause(string expressionBody)
        {
            ExpressionBody = expressionBody;
        }

        public string ExpressionBody { get; private set; }
    }
}
