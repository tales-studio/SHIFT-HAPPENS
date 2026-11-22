using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class SkipSequenceButton : MonoBehaviour
{
    [SerializeField] private Sequence sequence;
    [SerializeField] private Button button;

    private void Awake()
    {
        SetVisible(false);
    }

    private void OnEnable()
    {
        if (button != null) button.onClick.AddListener(Skip);
        if (sequence == null) return;

        sequence.Started += HandleStarted;
        sequence.Ended += HandleEnded;
        SetVisible(sequence.IsPlaying);
    }

    private void OnDisable()
    {
        if (button != null) button.onClick.RemoveListener(Skip);
        if (sequence == null) return;

        sequence.Started -= HandleStarted;
        sequence.Ended -= HandleEnded;
    }

    public void Skip()
    {
        if (sequence == null || !sequence.IsPlaying) return;
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

        sequence.FastForward();
    }

    private void HandleStarted()
    {
        SetVisible(true);
    }

    private void HandleEnded()
    {
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        if (button != null) button.gameObject.SetActive(visible);
    }
}
