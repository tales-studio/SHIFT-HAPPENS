using System;
using UnityEngine;
using UnityEngine.Events;

public class TaskLogic : MonoBehaviour
{
    [SerializeField] private TaskData task;
    [SerializeField] private bool canSpawnRandomly = true;

    private InteractableObject interactable;

    public event Action<bool> ActiveChanged;

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
        interactable.SetCanInteract(false);
    }

    private void SetActiveState(bool active)
    {
        isActive = active;
        ActiveChanged?.Invoke(active);
    }

    private void Start()
    {
        interactable.onInteract.AddListener(Complete);
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

        remainingTime = task.expirationSeconds;
        interactable.SetCanInteract(true);
        SetActiveState(true);
        Debug.Log(task.displayName);
    }

    public void Complete()
    {
        if (!isActive) return;

        interactable.SetCanInteract(false);
        SetActiveState(false);
        TaskManager.Instance.ReportCompleted(this);
    }

    private void Fail()
    {
        interactable.SetCanInteract(false);
        SetActiveState(false);
        TaskManager.Instance.ReportFailed(this);
    }


    public void ResetTask()
    {
        remainingTime = 0f;
        interactable.SetCanInteract(false);
        SetActiveState(false);
    }
}
