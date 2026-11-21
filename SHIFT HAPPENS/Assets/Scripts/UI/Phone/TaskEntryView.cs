using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class TaskEntryView : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private TMP_Text timeLabel;
    [SerializeField] private RectTransform timeBar;
    [SerializeField] private Image timeBarImage;
    [SerializeField] private GameObject newMarker;
    [SerializeField] private RectTransform newDot;
    [SerializeField, Min(0f)] private float dotGap = 7f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color urgentColor = new Color(0.9f, 0.2f, 0.2f);

    private TaskLogic task;
    private Action<TaskEntryView> onFinished;

    public RectTransform Rect => (RectTransform)transform;
    public bool IsNew { get; private set; }

    public void SetNew(bool isNew)
    {
        IsNew = isNew;
        if (newMarker != null) newMarker.SetActive(isNew);
        if (newDot != null) newDot.gameObject.SetActive(isNew);
        PlaceDot();
    }

    public void Bind(TaskLogic taskLogic, string createdAt, Action<TaskEntryView> finished)
    {
        task = taskLogic;
        onFinished = finished;

        label.text = task.Data.displayName;
        if (timeLabel != null) timeLabel.text = createdAt;
        timeBar.parent.gameObject.SetActive(task.Data.expirationSeconds > 0f);
        task.ActiveChanged += HandleActiveChanged;

        SetNew(true);

        UpdateTimeBar();
    }

    private void Update()
    {
        UpdateTimeBar();
    }

    private void OnDestroy()
    {
        if (task != null) task.ActiveChanged -= HandleActiveChanged;
    }

    private void PlaceDot()
    {
        if (newDot == null) return;

        float reserved = IsNew ? newDot.rect.width + dotGap : 0f;
        label.margin = new Vector4(0f, 0f, reserved, 0f);

        float available = label.rectTransform.rect.width - reserved;
        float textWidth = Mathf.Min(label.GetPreferredValues(label.text).x, available);
        newDot.anchoredPosition = new Vector2(textWidth + dotGap, 0f);
    }

    private void UpdateTimeBar()
    {
        if (task == null) return;

        float remaining = task.NormalizedTime;
        timeBar.anchorMax = new Vector2(remaining, 1f);
        timeBarImage.color = Color.Lerp(urgentColor, normalColor, remaining);
    }

    private void HandleActiveChanged(bool active)
    {
        if (active) return;

        task.ActiveChanged -= HandleActiveChanged;
        task = null;
        onFinished?.Invoke(this);
    }
}
