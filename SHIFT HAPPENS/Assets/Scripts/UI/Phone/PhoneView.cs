using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PhoneView : MonoBehaviour
{
    [SerializeField] private RectTransform body;
    [SerializeField] private Vector2 hiddenPosition = new Vector2(-40f, -720f);
    [SerializeField] private Vector2 shownPosition = new Vector2(-40f, 40f);
    [SerializeField, Min(0f)] private float slideDuration = 0.25f;

    [Header("Sounds")]
    [SerializeField] private SoundId openSound = SoundId.PhoneOpen;
    [SerializeField] private SoundId closeSound = SoundId.PhoneClose;

    [Header("Input")]
    [SerializeField] private InputActionReference toggleAction;

    private GameManager gameManager;
    private Coroutine slideRoutine;
    private readonly HashSet<object> locks = new HashSet<object>();

    public bool IsOpen { get; private set; }
    public bool IsLocked => locks.Count > 0;

    public event Action<bool> OnOpenChanged;

    public void AddLock(object source)
    {
        locks.Add(source);
    }

    public void RemoveLock(object source)
    {
        locks.Remove(source);
    }

    private void Awake()
    {
        body.anchoredPosition = hiddenPosition;
    }

    private void OnEnable()
    {
        toggleAction.action.performed += HandleToggle;
    }

    private void OnDisable()
    {
        toggleAction.action.performed -= HandleToggle;
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager != null) gameManager.OnShiftEnded += HandleShiftEnded;
    }

    private void OnDestroy()
    {
        if (gameManager != null) gameManager.OnShiftEnded -= HandleShiftEnded;
    }

    public void SetOpen(bool open)
    {
        if (IsOpen == open) return;

        IsOpen = open;
        AudioServices.Sfx.Play(open ? openSound : closeSound);

        if (slideRoutine != null) StopCoroutine(slideRoutine);
        slideRoutine = StartCoroutine(Slide(open ? shownPosition : hiddenPosition));

        OnOpenChanged?.Invoke(open);
    }

    private void HandleToggle(InputAction.CallbackContext context)
    {
        if (IsLocked) return;

        SetOpen(!IsOpen);
    }

    private void HandleShiftEnded(ShiftResult result)
    {
        SetOpen(false);
    }

    private IEnumerator Slide(Vector2 target)
    {
        Vector2 start = body.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / slideDuration));
            body.anchoredPosition = Vector2.Lerp(start, target, progress);
            yield return null;
        }

        body.anchoredPosition = target;
        slideRoutine = null;
    }
}
