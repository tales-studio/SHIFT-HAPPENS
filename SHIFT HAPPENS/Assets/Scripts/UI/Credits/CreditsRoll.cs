using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class CreditsRoll : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CreditsData data;

    [Header("Layout")]
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    [SerializeField] private TMP_Text headingTemplate;
    [SerializeField] private TMP_Text roleTemplate;
    [SerializeField] private TMP_Text namesTemplate;
    [SerializeField] private TMP_Text closingTemplate;
    [SerializeField, Min(0f)] private float sectionSpacing = 120f;
    [SerializeField, Min(0f)] private float entrySpacing = 40f;

    [Header("Playback")]
    [SerializeField, Min(0f)] private float startDelay = 0.5f;
    [SerializeField, Min(0f)] private float scrollSpeed = 35f;
    [SerializeField, Min(1f)] private float fastForwardMultiplier = 4f;
    [SerializeField] private InputActionReference fastForwardAction;
    [SerializeField, Min(0f)] private float closingHoldSeconds = 3f;

    [Header("Manual Scroll")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private InputActionReference scrollAction;
    [SerializeField, Min(0f)] private float manualScrollSpeed = 600f;

    [Header("End")]
    [SerializeField] private UnityEvent onFinished;

    private readonly List<GameObject> spawned = new List<GameObject>();
    private RectTransform closingInstance;
    private float edgePadding;
    private float endY;
    private float delayTimer;
    private float holdTimer;
    private bool finished;

    public bool IsFinished => finished;

    private void Awake()
    {
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);

        HideTemplate(headingTemplate);
        HideTemplate(roleTemplate);
        HideTemplate(namesTemplate);
        HideTemplate(closingTemplate);
    }

    private void OnEnable()
    {
        Build();
        Restart();
    }

    private void Update()
    {
        if (finished)
        {
            ManualScroll();
            return;
        }

        float deltaTime = Time.unscaledDeltaTime;

        if (delayTimer > 0f)
        {
            delayTimer -= deltaTime;
            return;
        }

        float speed = scrollSpeed * (IsFastForwarding() ? fastForwardMultiplier : 1f);
        Vector2 position = content.anchoredPosition;

        if (position.y < endY)
        {
            position.y = Mathf.Min(position.y + speed * deltaTime, endY);
            content.anchoredPosition = position;
            return;
        }

        holdTimer += deltaTime * (IsFastForwarding() ? fastForwardMultiplier : 1f);
        if (closingInstance == null || holdTimer >= closingHoldSeconds) Finish();
    }

    public void Restart()
    {
        if (scrollRect != null) scrollRect.enabled = false;
        SetScrollbarVisible(false);

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        float viewportHeight = viewport.rect.height;
        float contentHeight = content.rect.height - edgePadding;

        endY = closingInstance != null
            ? contentHeight - (viewportHeight + closingInstance.rect.height) * 0.5f
            : contentHeight;

        content.anchoredPosition = new Vector2(content.anchoredPosition.x, edgePadding - viewportHeight);
        delayTimer = startDelay;
        holdTimer = 0f;
        finished = false;
    }

    public void Build()
    {
        Clear();
        ApplyEdgePadding();
        if (data == null) return;

        for (int i = 0; i < data.sections.Count; i++)
        {
            CreditsData.Section section = data.sections[i];
            if (i > 0) Spacer(sectionSpacing);

            if (!string.IsNullOrWhiteSpace(section.heading))
            {
                Spawn(headingTemplate, section.heading);
                Spacer(entrySpacing);
            }

            for (int j = 0; j < section.entries.Count; j++)
            {
                CreditsData.Entry entry = section.entries[j];
                if (j > 0) Spacer(entrySpacing);

                if (!string.IsNullOrWhiteSpace(entry.role)) Spawn(roleTemplate, entry.role);
                if (entry.names.Count > 0) Spawn(namesTemplate, string.Join("\n", entry.names));
            }
        }

        if (!string.IsNullOrWhiteSpace(data.closingMessage))
        {
            Spacer(sectionSpacing);
            TMP_Text closing = Spawn(closingTemplate != null ? closingTemplate : headingTemplate, data.closingMessage);
            closingInstance = closing != null ? closing.rectTransform : null;
        }
    }

    private void Finish()
    {
        finished = true;
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, 0f);

        if (scrollRect != null)
        {
            scrollRect.enabled = true;
            scrollRect.StopMovement();
        }

        SetScrollbarVisible(true);
        onFinished.Invoke();
    }

    private void SetScrollbarVisible(bool visible)
    {
        if (scrollRect != null && scrollRect.verticalScrollbar != null)
            scrollRect.verticalScrollbar.gameObject.SetActive(visible);
    }

    private void ApplyEdgePadding()
    {
        RectMask2D mask = viewport.GetComponent<RectMask2D>();
        edgePadding = mask != null ? mask.softness.y : 0f;

        VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
        if (layout == null) return;

        int padding = Mathf.RoundToInt(edgePadding);
        layout.padding = new RectOffset(layout.padding.left, layout.padding.right, padding, padding);
    }

    private void ManualScroll()
    {
        if (scrollRect == null || scrollAction == null || scrollAction.action == null) return;

        float input = scrollAction.action.ReadValue<Vector2>().y;
        if (Mathf.Approximately(input, 0f)) return;

        float range = content.rect.height - viewport.rect.height;
        if (range <= 0f) return;

        float delta = input * manualScrollSpeed * Time.unscaledDeltaTime / range;
        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition + delta);
    }

    private bool IsFastForwarding()
    {
        return fastForwardAction != null && fastForwardAction.action != null && fastForwardAction.action.IsPressed();
    }

    private TMP_Text Spawn(TMP_Text template, string text)
    {
        if (template == null) return null;

        TMP_Text instance = Instantiate(template, content);
        instance.text = text;
        instance.gameObject.SetActive(true);
        spawned.Add(instance.gameObject);
        return instance;
    }

    private void Spacer(float height)
    {
        if (height <= 0f) return;

        GameObject spacer = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
        spacer.transform.SetParent(content, false);
        spacer.GetComponent<LayoutElement>().minHeight = height;
        spawned.Add(spacer);
    }

    private void Clear()
    {
        foreach (GameObject item in spawned)
        {
            if (item == null) continue;
            item.SetActive(false);
            Destroy(item);
        }

        spawned.Clear();
        closingInstance = null;
    }

    private static void HideTemplate(TMP_Text template)
    {
        if (template != null) template.gameObject.SetActive(false);
    }
}
