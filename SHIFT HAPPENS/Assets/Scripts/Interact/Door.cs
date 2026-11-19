using System;
using UnityEngine;

[RequireComponent(typeof(InteractableObject))]
[RequireComponent(typeof(TwoStateVisual))]
public class Door : MonoBehaviour
{
    [SerializeField] private Collider2D blocker;

    [SerializeField] private bool startsOpen;

    [Tooltip("A locked door can't be opened until Unlock() is called.")]
    [SerializeField] private bool startsLocked;

    private InteractableObject interactable;
    private TwoStateVisual visual;

    public bool IsOpen { get; private set; }
    public bool IsLocked { get; private set; }

    public event Action<bool> OpenStateChanged;
    public event Action Unlocked;
    public event Action OpenBlockedByLock;

    private void Awake()
    {
        interactable = GetComponent<InteractableObject>();
        visual = GetComponent<TwoStateVisual>();

        IsOpen = startsOpen;
        IsLocked = startsLocked;

        if (blocker != null && blocker.isTrigger)
        {
            Debug.LogWarning($"{name}: the Door blocker collider is a trigger, so it won't block movement.", this);
        }
    }

    private void OnEnable()
    {
        interactable.onInteract.AddListener(Toggle);
    }

    private void OnDisable()
    {
        interactable.onInteract.RemoveListener(Toggle);
    }

    private void Start()
    {
        ApplyState();
    }

    public void Toggle()
    {
        if (IsOpen) Close();
        else TryOpen();
    }

    public bool TryOpen()
    {
        if (IsOpen) return true;

        if (IsLocked)
        {
            OpenBlockedByLock?.Invoke();
            return false;
        }

        SetOpen(true);
        return true;
    }

    public void Close()
    {
        SetOpen(false);
    }

    public void Unlock()
    {
        if (!IsLocked) return;

        IsLocked = false;
        Unlocked?.Invoke();
    }

    public void Lock()
    {
        IsLocked = true;
    }

    private void SetOpen(bool open)
    {
        if (open == IsOpen) return;

        IsOpen = open;
        ApplyState();
        OpenStateChanged?.Invoke(open);
    }

    private void ApplyState()
    {
        if (blocker != null) blocker.enabled = !IsOpen;
        visual.SetAlternateState(IsOpen);
    }
}
