using UnityEngine;

public class LockStep : SequenceStep
{
    [SerializeField] private LockTarget target;
    [SerializeField] private bool locked = true;

    public static void Apply(LockTarget target, bool locked, object owner)
    {
        if (!Lockables.TryGet(target, out ILockable lockable)) return;

        if (locked) lockable.AddLock(owner);
        else lockable.RemoveLock(owner);
    }

    protected override void OnEnter()
    {
        ApplyOwn();
        Complete();
    }

    protected override void OnFastForward()
    {
        ApplyOwn();
    }

    private void ApplyOwn()
    {
        Sequence owner = GetComponentInParent<Sequence>();
        Apply(target, locked, owner != null ? owner : (object)this);
    }
}
