using System;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace ViveHandTracking {

class ThreadYieldInstruction<T> : CustomYieldInstruction
    where T : struct {
  private Thread thread = null;
  private Func<T> func = null;
  public T? result { get; private set; }

  public ThreadYieldInstruction(Func<T> func) {
    this.func = func;
    result = null;
    thread = new Thread(Run);
    thread.Start();
  }

  private void Run() {
    try {
      result = func();
    } catch (Exception e) {
      Debug.LogError(e.ToString());
    }
    thread = null;
  }

  public override bool keepWaiting {
    get { return thread != null; }
  }
}

}
