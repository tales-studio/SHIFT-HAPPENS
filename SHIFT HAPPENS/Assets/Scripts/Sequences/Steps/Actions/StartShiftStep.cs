public class StartShiftStep : SequenceStep
{
    protected override void OnEnter()
    {
        StartShift();
        Complete();
    }

    protected override void OnFastForward()
    {
        StartShift();
    }

    private static void StartShift()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null) return;

        GamePhase phase = gameManager.CurrentPhase;
        if (phase == GamePhase.Menu || phase == GamePhase.Tutorial)
            gameManager.StartShift();
    }
}
