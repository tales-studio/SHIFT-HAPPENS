using UnityEngine;

[DisallowMultipleComponent]
public class SkipCutsceneHold : MonoBehaviour
{
    [SerializeField] private HoldToSkip hold;

    private ICutscenePlayer player;

    private void Start()
    {
        player = CutsceneServices.Player;
        player.Started += HandleStarted;
        player.Finished += HandleFinished;
    }

    private void OnDestroy()
    {
        if (player == null) return;

        player.Started -= HandleStarted;
        player.Finished -= HandleFinished;
    }

    private void HandleStarted(Cutscene cutscene)
    {
        if (cutscene.Skippable) hold.Show(cutscene.SkipHoldDuration, player.Skip);
    }

    private void HandleFinished(Cutscene cutscene)
    {
        hold.Hide();
    }
}
