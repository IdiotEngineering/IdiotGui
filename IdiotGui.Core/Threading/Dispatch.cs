using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

// What am I after here.

// In code, adding a [DispatchHook(CallFrequency.PerFrame, DispatchPhase.Update)
// At runtime, adding a DispatchHook, maybe recursively.
// Threading use cases. For example, each widget building a new Mesh after layout. This is easy to multi-thread.
//
// // This is per-instance, can't use 100% static stuff here, so register in the c-tor or via runtime Attribute lookup.
// // In Constructor:
// Dispatch.AddDispatchEventHandler(this.BuildMesh, CallFrequency.PerFrame, DispatchPhase.Update, DispatchTo.ThreadPool)
// Or:
// Dispatch.RegisterDynamic(this);

// [Dispatch(PerFrame = true, ThreadPool = true)]
// void BuildMesh() { ... }

// Running through the pre-masticated command buffer is actually just another event, that runs on the GPU only. Hmm.
// [GpuDispatch(PerFrame = true, order = Order.Late)]
// void GpuRenderBuffer() { ... }

// Then things like GPU buffer swaps can be done thusly:
// [CpuDispatch(PerFrame = true, Phase = Phase.Syncronized)]
// [GpuDispatch(PerFrame = true, Phase = Phase.Syncronized)]
// void BufferSwap() { ... }

namespace IdiotGui.Core.Threading
{
  public static class Dispatch
  {
    #region Types

    public enum Phase
    {
      First,
      Default,
      DrawCallEnqueue,
      Last,

      /// <summary>
      ///   Represents the time when the GPU thread is in lockstep with the system thread. During this Phase, only one thread
      ///   will be running at any point in time (including the GPU thread) so do work very sparingly here.
      /// </summary>
      Lockstep
    }

    private struct DispatchAction
    {
      #region Fields / Properties

      public bool IsPerFrame;
      public Action Action;

      #endregion
    }

    #endregion

    #region Fields / Properties

    /// <summary>
    ///   The thread pool will be synchronized BEFORE each phase in this list.
    /// </summary>
    public static Phase[] CpuSyncronizedPhases = {Phase.DrawCallEnqueue, Phase.Last};

    /// <summary>
    ///   The workforce for all GPU interfacing (OpenGL calls). This is a single-thread workforce.
    /// </summary>
    public static LockstepWorkforce GpuThreadWorkforce = LockstepWorkforce.FromThreadPool("GpuThread", 1);

    /// <summary>
    ///   The workforce for the Thread Pool, made up of N threads where N is the number of cores on the system.
    /// </summary>
    public static LockstepWorkforce ThreadPoolWorkforce = LockstepWorkforce.FromThreadPool("ThreadPool",
      Environment.ProcessorCount);

    private static readonly List<DispatchAction>[] CpuDispatches =
      Enumerable.Range(0, Enum.GetValues(typeof (Phase)).Length).Select(_ => new List<DispatchAction>()).ToArray();

    private static List<DispatchAction>[] _gpuDisaptches =
      Enumerable.Range(0, Enum.GetValues(typeof (Phase)).Length).Select(_ => new List<DispatchAction>()).ToArray();

    private static Phase _currentPhase = Phase.First;
    private static Thread _mainSystemThread;

    #endregion

    /// <summary>
    ///   Called to pass control off to Dispatch. This will never return.
    /// </summary>
    public static void CaptureMainThread()
    {
      _mainSystemThread = Thread.CurrentThread;
      _mainSystemThread.Name = "MainSystemThread";
      while (true)
      {
        // Iterate over Phases [First, Last]
        for (_currentPhase = Phase.First;
          (int) _currentPhase <= (int) Phase.Last;
          _currentPhase = (Phase) ((int) _currentPhase + 1))
        {
          if (CpuSyncronizedPhases.Contains(_currentPhase))
          {
            ThreadPoolWorkforce.AttainLockstep();
            ThreadPoolWorkforce.ReleaseLockstep();
          }
          ExecutePhase(CpuDispatches, _currentPhase);
        }
        // Synchronize with the ThreadPool and GPU thread (to prepare for Lockstep phase)
        ThreadPoolWorkforce.AttainLockstep();
        GpuThreadWorkforce.AttainLockstep();
        _currentPhase = Phase.Lockstep;
        // Do all CPU-Lockstep work now
        ExecutePhase(CpuDispatches, Phase.Lockstep);
        // Enqueue Lockstep GPU work and release the GPU lock
        GpuThreadWorkforce.EnqueueAction(() => ExecutePhase(_gpuDisaptches, Phase.Lockstep));
        GpuThreadWorkforce.ReleaseLockstep();
        // Re-attain the lock after the lockstep phase is done
        GpuThreadWorkforce.AttainLockstep();
        // Copy gpu dispatches and re-add per-frame actions
        var dispatches = _gpuDisaptches;
        _gpuDisaptches = new List<DispatchAction>[Enum.GetValues(typeof(Phase)).Length];
        for (var i = 0; i < _gpuDisaptches.Length; i++)
          _gpuDisaptches[i].AddRange(dispatches[i].Where(dispatchAction => dispatchAction.IsPerFrame));
        // Enqueue next frame's GPU work
        GpuThreadWorkforce.EnqueueAction(() =>
        {
          for (var phase = Phase.First; (int) phase <= (int) Phase.Last; phase = (Phase) ((int) phase + 1))
            ExecutePhase(dispatches, phase);
        });
        // Release the lock on the GPU to let it start rendering the next frame
        GpuThreadWorkforce.ReleaseLockstep();
      }
      // ReSharper disable once FunctionNeverReturns
    }

    //public static void AddDispatchEventHandler(Action handler, bool perFrame = false, Phase phase = Phase.Update,
    //  Order order = Order.Default)
    //{
    //  // This can only be called the main thread to keep the dependency spaghetti to a minimum.
    //  AssertMainThread();
    //  // Check if it also needs to be executed now
    //  if (phase == _currentPhase && order == _currentOrder)
    //  {
    //    // This is a recursive addition during the same phase/order. Just call the handler now.
    //    handler();
    //    // If it's also supposed to be called per-frame, add it to the recursive additions list.
    //    _recursiveAdditions.Add(handler);
    //  }
    //  else
    //  {
    //    // Add it to the correct buffer without calling it
    //    var buffer = perFrame ? PerFrameDisaptches : SingleFrameDispatches;
    //    var list = buffer[(int) phase][(int) order];
    //    list.Add(handler);
    //  }
    //}

    public static void SyncronizeThreadPool()
    {
      throw new NotImplementedException();
    }

    public static void AssertPhase(Phase phase)
    {
#if DEBUG
      if (_currentPhase != phase)
        throw new Exception("The current phase was expected to be " + phase + " but was " + _currentPhase);
#endif
    }

    public static void AssertMainThread()
    {
#if DEBUG
      if (Thread.CurrentThread != _mainSystemThread)
      {
        throw new Exception("The calling thread was expected to be the main thread, but was: " +
                            Thread.CurrentThread.Name);
      }
#endif
    }

    private static void ExecutePhase(IList<List<DispatchAction>> buffer, Phase phase)
    {
      var dispatchActions = buffer[(int) phase];
      buffer[(int) phase] = new List<DispatchAction>();
      foreach (var disaptchAction in dispatchActions)
      {
        // Re-add any per-frame actions.
        if (disaptchAction.IsPerFrame)
          buffer[(int) phase].Add(disaptchAction);
        disaptchAction.Action();
      }
    }
  }
}