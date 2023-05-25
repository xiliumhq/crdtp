namespace Xilium.Crdtp
{
    public class OutputItem
    {
        private OutputScope? _scope;

        public string Path { get; init; } = default!;
        public string Category { get; init; } = default!;
        public string Content { get; init; } = default!;

        internal string PhysicalPath => _scope!.GetPhysicalPath(this);

        internal void Attach(OutputScope scope)
        {
            Check.That(_scope == null);
            _scope = scope;
        }
    }
}
