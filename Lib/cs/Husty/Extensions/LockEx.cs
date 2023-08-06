namespace Husty.Extensions;

public static class LockEx
{
  public static void Safeguard(this SpinLock locker, Action action)
  {
    var gotLock = false;
    try { locker.Enter(ref gotLock); action(); }
    finally { if (gotLock) locker.Exit(); }
  }

  public static void SafeGuard(this SemaphoreSlim locker, Action action)
  {
    locker.Wait();
    try { action(); }
    finally { locker.Release(); }
  }

}
