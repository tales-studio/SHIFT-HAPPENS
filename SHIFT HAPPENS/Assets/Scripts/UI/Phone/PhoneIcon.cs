using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class PhoneIcon : MonoBehaviour
{
    [SerializeField] private PhoneView phone;
    [SerializeField] private RectTransform visuals;
    [SerializeField] private GameObject badge;
    [SerializeField] private TMP_Text badgeCount;
    [SerializeField, Min(1)] private int maxBadgeCount = 9;

    [Header("Buzz")]
    [SerializeField, Min(0f)] private float buzzAngle = 12f;
    [SerializeField, Min(0f)] private float buzzDuration = 0.5f;
    [SerializeField, Min(1)] private int buzzShakes = 4;

    private readonly HashSet<TaskLogic> unread = new HashSet<TaskLogic>();
    private TaskManager taskManager;
    private Coroutine buzzRoutine;

    private void Start()
    {
        phone.OnOpenChanged += HandleOpenChanged;

        taskManager = TaskManager.Instance;
        if (taskManager != null)
        {
            taskManager.OnTaskActivated += HandleTaskActivated;
            taskManager.OnTaskCompleted += HandleTaskFinished;
            taskManager.OnTaskFailed += HandleTaskFinished;
        }

        HandleOpenChanged(phone.IsOpen);
        UpdateBadge();
    }

    private void OnDestroy()
    {
        if (phone != null) phone.OnOpenChanged -= HandleOpenChanged;

        if (taskManager == null) return;

        taskManager.OnTaskActivated -= HandleTaskActivated;
        taskManager.OnTaskCompleted -= HandleTaskFinished;
        taskManager.OnTaskFailed -= HandleTaskFinished;
    }

    private void HandleTaskActivated(TaskLogic task)
    {
        if (phone.IsOpen) return;

        unread.Add(task);
        UpdateBadge();
        Buzz();
    }

    private void HandleTaskFinished(TaskLogic task)
    {
        if (unread.Remove(task)) UpdateBadge();
    }

    private void HandleOpenChanged(bool open)
    {
        visuals.gameObject.SetActive(!open);

        if (!open) return;

        StopBuzz();
        unread.Clear();
        UpdateBadge();
    }

    private void UpdateBadge()
    {
        badge.SetActive(unread.Count > 0);
        badgeCount.text = unread.Count > maxBadgeCount ? maxBadgeCount + "+" : unread.Count.ToString();
    }

    private void Buzz()
    {
        StopBuzz();
        buzzRoutine = StartCoroutine(Shake());
    }

    private void StopBuzz()
    {
        if (buzzRoutine != null) StopCoroutine(buzzRoutine);
        buzzRoutine = null;
        visuals.localRotation = Quaternion.identity;
    }

    private IEnumerator Shake()
    {
        float elapsed = 0f;

        while (elapsed < buzzDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / buzzDuration);
            float angle = Mathf.Sin(progress * buzzShakes * Mathf.PI * 2f) * buzzAngle * (1f - progress);
            visuals.localRotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        visuals.localRotation = Quaternion.identity;
        buzzRoutine = null;
    }
}
