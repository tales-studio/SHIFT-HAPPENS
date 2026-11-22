using UnityEngine;

public class HudStep : SequenceStep
{
    [SerializeField] private HudElement element;
    [SerializeField] private bool visible = true;
    [SerializeField] private bool instant;

    protected override void OnEnter()
    {
        Apply(instant);
        Complete();
    }

    protected override void OnFastForward()
    {
        Apply(true);
    }

    private void Apply(bool now)
    {
        HudGroup hudGroup = HudGroup.Find(element);
        if (hudGroup != null) hudGroup.SetVisible(visible, now);
    }
}
