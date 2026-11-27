using System.Collections.Generic;

public static class Lockables
{
    private static readonly Dictionary<LockTarget, ILockable> registered = new Dictionary<LockTarget, ILockable>();

    public static void Register(ILockable lockable)
    {
        registered[lockable.LockTarget] = lockable;
    }

    public static void Unregister(ILockable lockable)
    {
        if (registered.TryGetValue(lockable.LockTarget, out ILockable current) && current == lockable)
            registered.Remove(lockable.LockTarget);
    }

    public static bool TryGet(LockTarget target, out ILockable lockable)
    {
        return registered.TryGetValue(target, out lockable);
    }
}
