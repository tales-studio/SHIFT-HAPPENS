using UnityEngine;

public class NpcAppearStep : SequenceStep
{
    [SerializeField] private NpcWalkOut npc;

    protected override void OnEnter()
    {
        if (npc != null) npc.Appear();
        Complete();
    }

    protected override void OnFastForward()
    {
        if (npc != null) npc.Appear();
    }
}
