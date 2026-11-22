using UnityEngine;

public class NpcWalkOutStep : SequenceStep
{
    [SerializeField] private NpcWalkOut npc;
    [SerializeField] private Transform exit;

    protected override void OnEnter()
    {
        if (npc != null)
        {
            if (exit != null) npc.Leave(exit.position);
            else npc.Despawn();
        }

        Complete();
    }

    protected override void OnFastForward()
    {
        if (npc != null) npc.Despawn();
    }
}
