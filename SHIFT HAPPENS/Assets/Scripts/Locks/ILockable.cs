public interface ILockable
{
    LockTarget LockTarget { get; }
    bool IsLocked { get; }

    void AddLock(object source);
    void RemoveLock(object source);
}
