using System;
using UnityEngine;
using UnityEngine.Events;

public class TaskLogic : MonoBehaviour
{
    [SerializeField] private TaskData task;
    [SerializeField] private bool canSpawnRandomly = true;

    private InteractableObject interactable;

    public TaskData Data => task;
    public bool isActive { get; private set; }
    public bool CanSpawnRandomly => canSpawnRandomly;
    public float remainingTime { get; private set; }
    public float NormalizedTime
    {
        get
        {
            if (task == null || task.expirationSeconds <= 0f) 
                return 1f;
            else
                return Mathf.Clamp01(remainingTime / task.expirationSeconds);
        }
    }
    private void Awake()
    {
        interactable = GetComponent<InteractableObject>();
    }
    private void Start()
    {
        interactable.onInteract.AddListener(Complete);
        interactable.HideIndicators();
        TaskManager.Instance.Register(this);
    }

    private void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.onInteract.RemoveListener(Complete);
        }
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.Unregister(this);
        }
    }

    private void Update()
    {
        if (!isActive || task.expirationSeconds <= 0f) return;
        if (GameManager.Instance == null || !GameManager.Instance.isPlaying) return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f) 
            Fail();
    }

    public void Activate()
    {
        if (isActive || task == null) return;

        isActive = true;
        remainingTime = task.expirationSeconds;
        interactable.ShowIndicators();
    }

    public void Complete()
    {
        if (!isActive) return;

        isActive = false;
        interactable.HideIndicators();
    }

    private void Fail()
    {
        isActive = false;
        interactable.HideIndicators();
    }


    public void ResetTask()
    {
        isActive = false;
        remainingTime = 0f;
        interactable.HideIndicators();
    }
}
