using System;
using System.Threading;
using Xilium.Threading.Tasks;

namespace Xilium.Threading;

public abstract class TaskRunner {
  [ThreadStatic]
  private static TaskRunner? t_current;
  public static TaskRunner? Current => t_current;

  protected static void SetCurrent(TaskRunner? taskRunner) {
    t_current = taskRunner;
  }

  public event EventHandler<Exception>? UnhandledException;

  protected void OnUnhandledException(Exception exception) {
    UnhandledException?.Invoke(this, exception);
  }

  public abstract void PostTask(Action action);
  public abstract void PostTask(SendOrPostCallback action, object? state);

  public TaskRunnerAwaiter GetAwaiter() => new TaskRunnerAwaiter(this);
}
