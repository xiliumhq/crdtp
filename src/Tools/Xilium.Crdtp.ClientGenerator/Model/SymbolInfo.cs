namespace Xilium.Crdtp.Model
{
    public abstract class SymbolInfo
    {
        private readonly Context _context;

        protected SymbolInfo(Context context)
        {
            _context = context;
        }

        protected Context Context => _context;

        public abstract bool IsReachable { get; }
    }
}
