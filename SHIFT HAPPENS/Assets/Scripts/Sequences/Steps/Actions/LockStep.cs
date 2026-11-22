using UnityEngine;

public enum LockTarget
{
    Movement,
    Phone,
    Interaction
}

public class LockStep : SequenceStep
{
    [SerializeField] private LockTarget target;
    [SerializeField] private bool locked = true;

    public static void Apply(LockTarget target, bool locked, object owner)
    {
        Player player = Player.Instance;

        switch (target)
        {
            case LockTarget.Movement:
                if (player == null) return;
                if (locked) player.Movement.AddLock(owner);
                else player.Movement.RemoveLock(owner);
                break;
            case LockTarget.Phone:
                PhoneView phone = FindFirstObjectByType<PhoneView>();
                if (phone == null) return;
                if (locked) phone.AddLock(owner);
                else phone.RemoveLock(owner);
                break;
            case LockTarget.Interaction:
                if (player == null || player.Interaction == null) return;
                if (locked) player.Interaction.AddLock(owner);
                else player.Interaction.RemoveLock(owner);
                break;
        }
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
