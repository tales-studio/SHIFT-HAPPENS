using UnityEngine;

public class WaitForMoveStep : PromptedStep
{
    [SerializeField, Min(0f)] private float distance = 3f;

    private Vector2 lastPosition;
    private float travelled;

    protected override void BeginWait()
    {
        travelled = 0f;
        if (Player.Instance != null) lastPosition = Player.Instance.Position;
    }

    private void Update()
    {
        if (!IsRunning || Player.Instance == null) return;

        Vector2 position = Player.Instance.Position;
        travelled += Vector2.Distance(position, lastPosition);
        lastPosition = position;

        if (travelled >= distance) Complete();
    }
}
