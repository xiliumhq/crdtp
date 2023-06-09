using System.Threading;

namespace Xilium.Threading;

public abstract class SequencedTaskRunner : TaskRunner {
  public abstract SynchronizationContext GetSynchronizationContext();
}
