using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace Xilium.Threading;

// TODO: Support flow of ExecutionContext with PostTask/UnsafePostTask.
// TODO: Action<T> would be also nice to support.

public sealed class ThreadPoolSequencedTaskRunner : SequencedTaskRunner {
  private readonly ConcurrentQueue<WorkItem> _queue;
  private readonly SynchronizationContext _synchronizationContext;
  private readonly ThreadPoolWorkItem _threadPoolWorkItem;
  private int _scheduled;

  public ThreadPoolSequencedTaskRunner() {
    _queue = new ConcurrentQueue<WorkItem>();
    _threadPoolWorkItem = new ThreadPoolWorkItem(this);
    _synchronizationContext = new SynchronizationContextImpl(this);
  }

  public override SynchronizationContext GetSynchronizationContext() =>
      _synchronizationContext;

  public override void PostTask(Action action) {
    _queue.Enqueue(new WorkItem((object)action, null));
    ScheduleProcessingIfNeed();
  }

  public override void PostTask(SendOrPostCallback action, object? state) {
    _queue.Enqueue(new WorkItem((object)action, state));
    ScheduleProcessingIfNeed();
  }

  private void ScheduleProcessingIfNeed() {
    if (Interlocked.CompareExchange(ref _scheduled, 1, 0) != 0)
      return;

    // TODO: This never returns false, but might throw exception (i.e.
    // OutOfMemory). Probably need to take this into account, and reset flag
    // back.
    _ = ThreadPool.UnsafeQueueUserWorkItem(_threadPoolWorkItem,
                                           preferLocal: true);
  }

  private sealed class ThreadPoolWorkItem : IThreadPoolWorkItem {
    private readonly ThreadPoolSequencedTaskRunner _taskRunner;
    public ThreadPoolWorkItem(ThreadPoolSequencedTaskRunner taskRunner) {
      _taskRunner = taskRunner;
    }

    public void Execute() {
      var previousTaskRunner = TaskRunner.Current;
      var previousSynchronizationContext =
          System.Threading.SynchronizationContext.Current;

      SetCurrent(_taskRunner);
      // TODO: Note that flowing with ExecutionContext also setup
      // Synchronization Context.
      SynchronizationContext.SetSynchronizationContext(
          _taskRunner._synchronizationContext);

      try {
        var queue = _taskRunner._queue;

        while (queue.TryDequeue(out var workItem)) {
          var o = workItem._action;
          try {
            if (o is SendOrPostCallback sendOrPostCallback) {
              sendOrPostCallback(workItem._state);
            } else if (o is Action<object?> actionWithState) {
              actionWithState(workItem._state);
            } else if (o is Action action) {
              action();
            } else
              throw new InvalidOperationException("Unreacheable.");
          } catch (Exception exception) {
            // TODO: Capture stacktrace here?
            // TODO: If there is no subscriber, then don't hide exception, and
            // rethrow it to threadpool?
            _taskRunner.OnUnhandledException(exception);
          }
        }

        // TODO: Consider reimplement this with counters.

        // TODO: Consider move this into finally block. If OnUnhandledException
        // method will throw exception, then TaskRunner will stop process
        // messages.

        // There is race condition: new task can be queued, but processing is
        // not scheduled in ScheduleProcessingIfNeed call. So, we might need
        // re-schedule processing.
        Volatile.Write(ref _taskRunner._scheduled, 0);
        // Now other threads might schedule processing.
        // However, we will re-schedule only if queue is not empty.
        if (!queue.IsEmpty)
          _taskRunner.ScheduleProcessingIfNeed();
      } finally {
        SynchronizationContext.SetSynchronizationContext(
            previousSynchronizationContext);
        SetCurrent(previousTaskRunner);
      }
    }
  }

  private readonly struct WorkItem {
    internal readonly object _action;
    internal readonly object? _state;

    public WorkItem(object action, object? state) {
      _action = action;
      _state = state;
    }
  }

  private sealed class SynchronizationContextImpl
      : System.Threading.SynchronizationContext {
    private readonly ThreadPoolSequencedTaskRunner _taskRunner;

    public SynchronizationContextImpl(
        ThreadPoolSequencedTaskRunner taskRunner) {
      _taskRunner = taskRunner;

      // Can be used to notify about blocking waits.
      // SetWaitNotificationRequired();
    }

    public override void Send(SendOrPostCallback d, object? state) =>
        throw new NotSupportedException();

    public override void Post(SendOrPostCallback d,
                              object? state) => _taskRunner.PostTask(d, state);

    public override void OperationStarted() {}
    public override void OperationCompleted() {}

    // Will be called if SetWaitNotificationRequired() called.
    public override int Wait(nint[] waitHandles, bool waitAll,
                             int millisecondsTimeout) =>
        base.Wait(waitHandles, waitAll, millisecondsTimeout);

    public override SynchronizationContext CreateCopy() =>
        throw new NotSupportedException();
  }
}
