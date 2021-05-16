using System.Collections.Generic;

namespace Xilium.Crdtp
{
    public abstract class OutputScope
    {
        public abstract IReadOnlyCollection<OutputItem> Items { get; }

        public abstract void Add(OutputItem item);
        public abstract int Commit();

        public abstract string PhysicalPath { get; }
        internal abstract string GetPhysicalPath(OutputItem outputItem);
    }
}
