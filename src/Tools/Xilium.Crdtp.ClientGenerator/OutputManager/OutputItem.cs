namespace Xilium.Crdtp
{
    public class OutputItem
    {
        private OutputScope? _scope;

        public string Path { get; init; }
        public string Category { get; init; }
        public string Content { get; init; }

        internal string PhysicalPath => _scope.GetPhysicalPath(this);

        internal void Attach(OutputScope scope)
        {
            Check.That(_scope == null);
            _scope = scope;
        }
    }
}
