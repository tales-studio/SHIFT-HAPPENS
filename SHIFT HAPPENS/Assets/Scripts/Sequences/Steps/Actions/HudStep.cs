using UnityEngine;

public class HudStep : SequenceStep
{
    [SerializeField] private HudElement element;
    [SerializeField] private bool visible = true;
    [SerializeField] private bool instant;

    protected override void OnEnter()
    {
        HudGroup hudGroup = HudGroup.Find(element);
        if (hudGroup != null) hudGroup.SetVisible(visible, instant);
        Complete();
    }
}
