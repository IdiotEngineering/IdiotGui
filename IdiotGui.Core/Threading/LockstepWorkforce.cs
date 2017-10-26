using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace IdiotGui.Core.Threading
{
  /// <summary>
  ///   Represents a workforce (single thread, or thread pool) that can lockstep with another workforce.
  /// </summary>
  public class LockstepWorkforce
  {
    #region Fields / Properties

    /// <summary>
    ///   The name of the workforce (Same as the name of the system thread / preamble).
    /// </summary>
    public string Name;

    /// <summary>
    ///   All system threads that are a member of this workforce.
    /// </summary>
    public Thread[] SystemThreads;

    /// <summary>
    ///   If this LockstepWorkforce was created via FromControlPassoff, call this action from the thread-to-capture to pass
    ///   control off to the LockstepWorkforce. It is null otherwise.
    /// </summary>
    public Action ControlPassoff;

    private readonly object _monitor = new object();
    private readonly BlockingCollection<Action> _workQueue = new BlockingCollection<Action>();
    private bool _awaitingRelease = true;

    #endregion

    private LockstepWorkforce()
    {
    }

    /// <summary>
    ///   Creates an (uninitialized) LockstepWorkforce and sets the ControlPassoff lambda. Call this lambda from what
    ///   ever thread you wish to capture. The lambda will only return once the LockstepWorkforce has been gracefully shut
    ///   down. If the thread is never shut down, or if the Thread is forcefully terminated, the lambda will never return.
    /// </summary>
    public static LockstepWorkforce FromControlPassoff(string threadName)
    {
      var workforce = new LockstepWorkforce {Name = threadName};
      workforce.ControlPassoff = () =>
      {
        workforce.ControlPassoff = null;
        Thread.CurrentThread.Name = threadName;
        workforce.SystemThreads = new[] {Thread.CurrentThread};
        foreach (var action in workforce._workQueue.GetConsumingEnumerable()) action();
      };
      return workforce;
    }

    public static LockstepWorkforce FromThreadPool(string threadPreamble, int threadCount)
    {
      var workforce = new LockstepWorkforce
      {
        Name = threadPreamble,
        SystemThreads = new Thread[threadCount]
      };
      for (var i = 0; i < threadCount; i++)
      {
        // All threads do nothing but pull actions out of the queue. Synchronization is done via injection into the queue.
        var thread =
          new Thread(() => { foreach (var action in workforce._workQueue.GetConsumingEnumerable()) action(); })
          {
            Name = threadPreamble + i
          };
        thread.Start();
        workforce.SystemThreads[i] = thread;
      }
      return workforce;
    }

    /// <summary>
    ///   Attains a lockstep (synchronization) with another Lockstep Workforce.
    /// </summary>
    public void AttainLockstep()
    {
      _awaitingRelease = true;
      var cte = new CountdownEvent(SystemThreads.Length);
      for (var i = 0; i < SystemThreads.Length; i++)
      {
        _workQueue.Add(() =>
        {
          // Signal that this thread got the message
          cte.Signal();
          // Then sleep until we are re-awoken
          lock (_monitor)
          {
            while (_awaitingRelease) Monitor.Wait(_awaitingRelease);
          }
        });
      }
      cte.Wait();
    }

    /// <summary>
    ///   Release the lockstep (synchronization) on the workforce, freeing it to start completing it's tasks.
    /// </summary>
    public void ReleaseLockstep()
    {
      _awaitingRelease = false;
      Monitor.PulseAll(_monitor);
    }

    /// <summary>
    ///   Enqueue a single action into the workforce.
    /// </summary>
    public void EnqueueAction(Action action) => _workQueue.Add(action);

    /// <summary>
    ///   Asserts that the calling thread is a member of the workforce.
    /// </summary>
    private void AssertCurrent()
    {
#if DEBUG
      var currentThread = Thread.CurrentThread;
      if (!SystemThreads.Contains(currentThread))
        throw new Exception(
          "Expected the calling thread to be a member of the workforce, but was not. Calling thread: " +
          currentThread.Name + " Workforce: " + Name);
#endif
    }
  }
}