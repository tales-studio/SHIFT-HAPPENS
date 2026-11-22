using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Sequence))]
public class TutorialDirector : MonoBehaviour
{
    [SerializeField] private GameObject[] firstRunOnly = new GameObject[0];

    private readonly ITutorialProgress progress = new PlayerPrefsTutorialProgress();
    private Sequence sequence;
    private bool replaying;

    public bool CanReplay
    {
        get
        {
            GameManager gameManager = GameManager.Instance;
            return gameManager != null && !sequence.IsPlaying && gameManager.IsShiftRunning;
        }
    }

    private void Awake()
    {
        sequence = GetComponent<Sequence>();
    }

    public void Begin()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null || sequence.IsPlaying) return;

        SetFirstRunOnly(true);

        if (progress.HasSeen)
        {
            sequence.FastForward(Finish);
            return;
        }

        gameManager.StartTutorial();
        sequence.Play(Finish);
    }

    public void Replay()
    {
        if (!CanReplay) return;

        GameManager gameManager = GameManager.Instance;
        gameManager.Resume();
        gameManager.AddShiftHold(this);

        replaying = true;
        SetFirstRunOnly(false);
        sequence.Play(Finish);
    }

    private void Finish()
    {
        progress.MarkSeen();

        GameManager gameManager = GameManager.Instance;

        if (replaying)
        {
            replaying = false;
            SetFirstRunOnly(true);
            if (gameManager != null) gameManager.RemoveShiftHold(this);
            return;
        }

        if (gameManager == null) return;

        GamePhase phase = gameManager.CurrentPhase;
        if (phase == GamePhase.Menu || phase == GamePhase.Tutorial)
            gameManager.StartShift();
    }

    private void SetFirstRunOnly(bool active)
    {
        foreach (GameObject target in firstRunOnly)
        {
            if (target != null) target.SetActive(active);
        }
    }

    [ContextMenu("Reset Tutorial Progress")]
    private void ResetProgress()
    {
        progress.Reset();
    }
}
