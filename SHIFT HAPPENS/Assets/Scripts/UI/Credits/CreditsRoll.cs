using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DisallowMultipleComponent]
[DefaultExecutionOrder(100)]
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
    [SerializeField] private TMP_Text tasksTemplate;
    [SerializeField] private TMP_Text closingTemplate;
    [SerializeField] private Image logoTemplate;
    [SerializeField] private string taskSeparator = " · ";
    [SerializeField, Min(0f)] private float columnGap = 60f;
    [SerializeField, Min(0f)] private float sectionSpacing = 120f;
    [SerializeField, Min(0f)] private float entrySpacing = 40f;
    [SerializeField, Min(0f)] private float memberSpacing = 16f;
    [SerializeField, Min(0f)] private float logoSpacing = 40f;

    [Header("Playback")]
    [SerializeField, Min(0f)] private float startDelay = 0.5f;
    [SerializeField, Min(0f)] private float scrollSpeed = 30f;
    [SerializeField, Min(1f)] private float fastForwardMultiplier = 4f;
    [SerializeField] private InputActionReference fastForwardAction;
    [SerializeField, Min(0f)] private float closingHoldSeconds = 1f;

    [Header("Manual Scroll")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private InputActionReference scrollAction;
    [SerializeField, Min(0f)] private float manualScrollSpeed = 600f;

    [Header("End")]
    [SerializeField] private UnityEvent onFinished;

    private struct StickyHeading
    {
        public RectTransform row;
        public RectTransform heading;
        public float height;
    }

    private readonly List<GameObject> spawned = new List<GameObject>();
    private readonly List<StickyHeading> stickyHeadings = new List<StickyHeading>();
    private RectTransform closingInstance;
    private LayoutElement closingGap;
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
        HideTemplate(tasksTemplate);
        HideTemplate(closingTemplate);
        HideTemplate(logoTemplate);
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

    private void LateUpdate()
    {
        UpdateStickyHeadings();
    }

    public void Restart()
    {
        if (scrollRect != null) scrollRect.enabled = false;
        SetScrollbarVisible(false);

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        float viewportHeight = viewport.rect.height;

        if (closingGap != null && closingInstance != null)
        {
            closingGap.minHeight = Mathf.Max(sectionSpacing, (viewportHeight - closingInstance.rect.height) * 0.5f);
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        }

        float contentHeight = content.rect.height - edgePadding;

        endY = closingInstance != null
            ? contentHeight - (viewportHeight + closingInstance.rect.height) * 0.5f
            : contentHeight;

        content.anchoredPosition = new Vector2(content.anchoredPosition.x, edgePadding - viewportHeight);
        delayTimer = startDelay;
        holdTimer = 0f;
        finished = false;

        UpdateStickyHeadings();
    }

    public void Build()
    {
        Clear();
        ApplyEdgePadding();
        if (data == null) return;

        for (int i = 0; i < data.sections.Count; i++)
        {
            if (i > 0) Track(Spacer(content, sectionSpacing));
            BuildSection(data.sections[i]);
        }

        bool hasMessage = !string.IsNullOrWhiteSpace(data.closingMessage);
        if (!hasMessage && logoTemplate == null) return;

        GameObject gap = Spacer(content, Mathf.Max(1f, sectionSpacing));
        Track(gap);
        closingGap = gap.GetComponent<LayoutElement>();

        closingInstance = CreateClosingBlock();
        Track(closingInstance.gameObject);

        if (hasMessage) Spawn(closingTemplate != null ? closingTemplate : headingTemplate, data.closingMessage, closingInstance);
        if (logoTemplate != null) SpawnLogo(closingInstance);
    }

    private RectTransform CreateClosingBlock()
    {
        RectTransform block = CreateChild("Closing", content);
        block.pivot = new Vector2(0.5f, 1f);

        VerticalLayoutGroup layout = block.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = logoSpacing;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        return block;
    }

    private void SpawnLogo(Transform parent)
    {
        Image logo = Instantiate(logoTemplate, parent);
        logo.gameObject.SetActive(true);

        if (data.studioLogo == null) return;

        logo.sprite = data.studioLogo;
        logo.type = Image.Type.Simple;
        logo.color = Color.white;
        logo.preserveAspect = true;

        foreach (TMP_Text label in logo.GetComponentsInChildren<TMP_Text>(true))
            label.gameObject.SetActive(false);
    }

    private void BuildSection(CreditsData.Section section)
    {
        RectTransform row = CreateRow();
        Track(row.gameObject);

        float columnWidth = Mathf.Max(0f, (content.rect.width - columnGap) * 0.5f);

        RectTransform headingColumn = CreateChild("HeadingColumn", row);
        LayoutElement headingElement = headingColumn.gameObject.AddComponent<LayoutElement>();
        headingElement.flexibleWidth = 1f;

        RectTransform entriesColumn = CreateChild("EntriesColumn", row);
        entriesColumn.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
        VerticalLayoutGroup entriesLayout = entriesColumn.gameObject.AddComponent<VerticalLayoutGroup>();
        entriesLayout.childAlignment = TextAnchor.UpperLeft;
        entriesLayout.childControlWidth = true;
        entriesLayout.childControlHeight = true;
        entriesLayout.childForceExpandWidth = true;
        entriesLayout.childForceExpandHeight = false;

        TMP_Text heading = string.IsNullOrWhiteSpace(section.heading) ? null : Spawn(headingTemplate, section.heading, headingColumn);
        if (heading != null)
        {
            heading.horizontalAlignment = HorizontalAlignmentOptions.Right;
            float height = heading.GetPreferredValues(section.heading, columnWidth, 0f).y;
            headingElement.minHeight = height;

            RectTransform headingRect = heading.rectTransform;
            headingRect.anchorMin = new Vector2(0f, 1f);
            headingRect.anchorMax = new Vector2(1f, 1f);
            headingRect.pivot = new Vector2(0.5f, 1f);
            headingRect.sizeDelta = new Vector2(0f, height);
            headingRect.anchoredPosition = Vector2.zero;

            stickyHeadings.Add(new StickyHeading { row = row, heading = headingRect, height = height });
        }

        for (int j = 0; j < section.entries.Count; j++)
        {
            CreditsData.Entry entry = section.entries[j];
            if (j > 0) Spacer(entriesColumn, entrySpacing);

            if (!string.IsNullOrWhiteSpace(entry.role)) AlignLeft(Spawn(roleTemplate, entry.role, entriesColumn));

            for (int k = 0; k < entry.members.Count; k++)
            {
                CreditsData.Member member = entry.members[k];
                if (k > 0) Spacer(entriesColumn, memberSpacing);

                if (!string.IsNullOrWhiteSpace(member.name)) AlignLeft(Spawn(namesTemplate, member.name, entriesColumn));

                string tasks = JoinTasks(member.tasks);
                if (tasks.Length > 0) AlignLeft(Spawn(tasksTemplate != null ? tasksTemplate : roleTemplate, tasks, entriesColumn));
            }
        }
    }

    private string JoinTasks(List<string> tasks)
    {
        List<string> filled = new List<string>();
        foreach (string task in tasks)
            if (!string.IsNullOrWhiteSpace(task)) filled.Add(task.Trim());

        return string.Join(taskSeparator, filled);
    }

    private void UpdateStickyHeadings()
    {
        float scroll = content.anchoredPosition.y;

        foreach (StickyHeading sticky in stickyHeadings)
        {
            if (sticky.row == null) continue;

            float rowTop = -sticky.row.anchoredPosition.y;
            float maxOffset = Mathf.Max(0f, sticky.row.rect.height - sticky.height);
            float offset = Mathf.Clamp(scroll + edgePadding - rowTop, 0f, maxOffset);
            sticky.heading.anchoredPosition = new Vector2(0f, -offset);
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

    private RectTransform CreateRow()
    {
        RectTransform row = CreateChild("Section", content);
        row.pivot = new Vector2(0.5f, 1f);

        HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = columnGap;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;
        return row;
    }

    private RectTransform CreateChild(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    private TMP_Text Spawn(TMP_Text template, string text, Transform parent)
    {
        if (template == null) return null;

        TMP_Text instance = Instantiate(template, parent);
        instance.text = text;
        instance.gameObject.SetActive(true);
        return instance;
    }

    private static void AlignLeft(TMP_Text text)
    {
        if (text != null) text.horizontalAlignment = HorizontalAlignmentOptions.Left;
    }

    private GameObject Spacer(Transform parent, float height)
    {
        if (height <= 0f) return null;

        RectTransform spacer = CreateChild("Spacer", parent);
        spacer.gameObject.AddComponent<LayoutElement>().minHeight = height;
        return spacer.gameObject;
    }

    private void Track(GameObject item)
    {
        if (item != null) spawned.Add(item);
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
        stickyHeadings.Clear();
        closingInstance = null;
        closingGap = null;
    }

    private static void HideTemplate(Component template)
    {
        if (template != null) template.gameObject.SetActive(false);
    }
}
