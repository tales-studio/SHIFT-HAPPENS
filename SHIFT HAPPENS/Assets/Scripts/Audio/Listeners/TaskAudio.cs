using System.Collections.Generic;
using UnityEngine;

public class TaskAudio : MonoBehaviour
{
    [SerializeField] private SoundId taskActivated = SoundId.UITaskNew;
    [SerializeField] private SoundId taskCompleted = SoundId.UITaskCompleted;
    [SerializeField] private SoundId taskFailed = SoundId.UITaskFailed;

    [Header("Expiring warning")]
    [SerializeField] private SoundId taskExpiring = SoundId.UITaskExpiring;
    [SerializeField, Range(0f, 1f)] private float expiringThreshold = 0.25f;

    private readonly List<TaskLogic> watched = new List<TaskLogic>();
    private readonly HashSet<TaskLogic> warned = new HashSet<TaskLogic>();
    private TaskManager subscribedTo;

    private void Start()
    {
        subscribedTo = TaskManager.Instance;
        if (subscribedTo == null)
        {
            Debug.LogWarning("TaskAudio: no TaskManager in the scene.", this);
            return;
        }

        subscribedTo.OnTaskActivated += HandleActivated;
        subscribedTo.OnTaskCompleted += HandleCompleted;
        subscribedTo.OnTaskFailed += HandleFailed;
    }

    private void OnDestroy()
    {
        if (subscribedTo == null) return;
        subscribedTo.OnTaskActivated -= HandleActivated;
        subscribedTo.OnTaskCompleted -= HandleCompleted;
        subscribedTo.OnTaskFailed -= HandleFailed;
    }

    private void Update()
    {
        for (int i = watched.Count - 1; i >= 0; i--)
        {
            TaskLogic task = watched[i];

            if (task == null || !task.isActive)
            {
                watched.RemoveAt(i);
                warned.Remove(task);
                continue;
            }

            if (!warned.Contains(task) && task.NormalizedTime <= expiringThreshold)
            {
                warned.Add(task);
                AudioServices.Sfx.Play(taskExpiring);
            }
        }
    }

    private void HandleActivated(TaskLogic task)
    {
        if (!watched.Contains(task)) watched.Add(task);
        AudioServices.Sfx.Play(taskActivated);
    }

    private void HandleCompleted(TaskLogic task) => AudioServices.Sfx.Play(taskCompleted);

    private void HandleFailed(TaskLogic task) => AudioServices.Sfx.Play(taskFailed);
}
