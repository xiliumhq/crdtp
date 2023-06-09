using System;
using System.Runtime.CompilerServices;

namespace Xilium.Threading.Tasks;

public readonly struct TaskRunnerAwaiter : ICriticalNotifyCompletion {
  private readonly TaskRunner _taskRunner;

  public TaskRunnerAwaiter(TaskRunner taskRunner) {
    _taskRunner = taskRunner;
  }

  public bool IsCompleted => TaskRunner.Current == _taskRunner;

  public void OnCompleted(Action continuation) {
    // TODO: Implement posting task with flowing ExecutionContext
    _taskRunner.PostTask(continuation);
  }

  public void UnsafeOnCompleted(Action continuation) {
    _taskRunner.PostTask(continuation);
  }

  public void GetResult() {}
}
