using UnityEngine;

public class AdvanceClockStep : SequenceStep
{
    [SerializeField, Min(0f)] private float minutes = 15f;

    protected override void OnEnter()
    {
        Advance();
        Complete();
    }

    protected override void OnFastForward()
    {
        Advance();
    }

    private void Advance()
    {
        ShiftClock clock = GameManager.Instance != null ? GameManager.Instance.Clock : null;
        if (clock != null) clock.AdvanceTime(minutes);
    }
}
