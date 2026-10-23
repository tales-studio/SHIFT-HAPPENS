using System.Collections.Generic;
using System;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }

    [SerializeField] private float firstTaskDelay;
    [SerializeField] private float minSpawnInterval;
    [SerializeField] private float maxSpawnInterval;
    [SerializeField] private int maxActiveTasks;

    private readonly List<TaskLogic> allTasks = new List<TaskLogic>();

    private float spawnTimer;

    private readonly List<TaskLogic> candidates = new List<TaskLogic>();

    public event Action<TaskLogic> OnTaskActivated;
    public event Action<TaskLogic> OnTaskCompleted;
    public event Action<TaskLogic> OnTaskFailed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        spawnTimer = firstTaskDelay;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isPlaying) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer > 0f) return;

        spawnTimer = UnityEngine.Random.Range(minSpawnInterval, maxSpawnInterval);
        TrySpawnRandomTask();

    }

    public void Register(TaskLogic logic)
    {
        if (!allTasks.Contains(logic))
        {
            allTasks.Add(logic);
        }
    }

    public void Unregister(TaskLogic logic)
    {
        allTasks.Remove(logic);
    }

    public void ClearTasks()
    {
        foreach (TaskLogic logic in allTasks)
        {
            logic.ResetTask();
        }
        spawnTimer = firstTaskDelay;
    }

    private void TrySpawnRandomTask()
    {
        int activeCount = 0;
        candidates.Clear();

        foreach (TaskLogic logic in allTasks)
        {
            if (logic.isActive)
            {
                activeCount++;
            }
            else if (logic.CanSpawnRandomly)
            {
                candidates.Add(logic);
            }
        }

        if (activeCount >= maxActiveTasks || candidates.Count == 0) return;

        TaskLogic chosen = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        chosen.Activate();
    }

}
