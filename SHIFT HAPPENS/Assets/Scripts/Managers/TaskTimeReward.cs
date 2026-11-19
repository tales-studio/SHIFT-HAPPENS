using UnityEngine;

public class TaskTimeReward : MonoBehaviour
{
    [SerializeField] private ShiftClock clock;

    private void Start()
    {
        if (clock == null && GameManager.Instance != null)
            clock = GameManager.Instance.Clock;

        if (TaskManager.Instance != null)
            TaskManager.Instance.OnTaskCompleted += HandleTaskCompleted;
    }

    private void OnDestroy()
    {
        if (TaskManager.Instance != null)
            TaskManager.Instance.OnTaskCompleted -= HandleTaskCompleted;
    }

    private void HandleTaskCompleted(TaskLogic task)
    {
        if (!enabled || clock == null || task == null || task.Data == null) return;

        clock.AdvanceTime(task.Data.minutesGainedOnComplete);
    }
}