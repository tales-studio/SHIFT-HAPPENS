using UnityEngine;

[DisallowMultipleComponent]
public class SkipSequenceHold : MonoBehaviour
{
    [SerializeField] private Sequence sequence;
    [SerializeField] private HoldToSkip hold;
    [SerializeField, Min(0f)] private float holdDuration = 1f;

    private void OnEnable()
    {
        if (sequence == null) return;

        sequence.Started += HandleStarted;
        sequence.Ended += HandleEnded;
        if (sequence.IsPlaying) HandleStarted();
    }

    private void OnDisable()
    {
        if (sequence == null) return;

        sequence.Started -= HandleStarted;
        sequence.Ended -= HandleEnded;
        if (hold != null) hold.Hide();
    }

    public void Skip()
    {
        if (sequence == null || !sequence.IsPlaying) return;

        sequence.FastForward();
    }

    private void HandleStarted()
    {
        hold.Show(holdDuration, Skip, IsPaused);
    }

    private void HandleEnded()
    {
        hold.Hide();
    }

    private static bool IsPaused()
    {
        return GameManager.Instance != null && GameManager.Instance.IsPaused;
    }
}
