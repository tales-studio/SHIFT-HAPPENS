using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Sequence))]
public class TutorialDirector : MonoBehaviour
{
    private Sequence sequence;

    private void Awake()
    {
        sequence = GetComponent<Sequence>();
    }

    public void Begin()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null || sequence.IsPlaying) return;

        gameManager.StartTutorial();
        sequence.Play(Finish);
    }

    private void Finish()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null && gameManager.CurrentPhase == GamePhase.Tutorial)
            gameManager.StartShift();
    }
}
