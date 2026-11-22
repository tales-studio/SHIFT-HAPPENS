using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Sequence))]
public class TutorialDirector : MonoBehaviour
{
    private readonly ITutorialProgress progress = new PlayerPrefsTutorialProgress();
    private Sequence sequence;

    private void Awake()
    {
        sequence = GetComponent<Sequence>();
    }

    public void Begin()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null || sequence.IsPlaying) return;

        if (progress.HasSeen)
        {
            sequence.FastForward(Finish);
            return;
        }

        gameManager.StartTutorial();
        sequence.Play(Finish);
    }

    private void Finish()
    {
        progress.MarkSeen();

        GameManager gameManager = GameManager.Instance;
        if (gameManager == null) return;

        GamePhase phase = gameManager.CurrentPhase;
        if (phase == GamePhase.Menu || phase == GamePhase.Tutorial)
            gameManager.StartShift();
    }

    [ContextMenu("Reset Tutorial Progress")]
    private void ResetProgress()
    {
        progress.Reset();
    }
}
