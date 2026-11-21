using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class TaskListView : MonoBehaviour
{
    private class LocationGroup
    {
        public string Name;
        public TMP_Text Header;
        public readonly List<TaskEntryView> Entries = new List<TaskEntryView>();
    }

    private struct Row
    {
        public RectTransform Rect;
        public float Height;
        public float X;
    }

    [SerializeField] private PhoneView phone;
    [SerializeField] private TMP_Text headerTemplate;
    [SerializeField] private TaskEntryView entryTemplate;
    [SerializeField] private GameObject emptyMessage;
    [SerializeField] private string unknownLocation = "Elsewhere";
    [SerializeField] private Color countColor = new Color(0.5f, 0.53f, 0.66f);
    [SerializeField, Min(0f)] private float headerHeight = 44f;
    [SerializeField, Min(0f)] private float entryHeight = 86f;
    [SerializeField] private GameObject scrollBar;
    [SerializeField] private RectTransform scrollThumb;
    [SerializeField] private InputAction scrollAction = new InputAction("ScrollTasks", InputActionType.Value, "<Mouse>/scroll/y");

    private readonly List<LocationGroup> groups = new List<LocationGroup>();
    private readonly List<Row> rows = new List<Row>();
    private TaskManager taskManager;
    private float top;
    private float headerX;
    private float entryX;
    private int scrollIndex;

    private void Awake()
    {
        top = headerTemplate.rectTransform.anchoredPosition.y;
        headerX = headerTemplate.rectTransform.anchoredPosition.x;
        entryX = entryTemplate.Rect.anchoredPosition.x;

        headerTemplate.gameObject.SetActive(false);
        entryTemplate.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        scrollAction.Enable();
    }

    private void OnDisable()
    {
        scrollAction.Disable();
    }

    private void Start()
    {
        taskManager = TaskManager.Instance;
        if (taskManager != null) taskManager.OnTaskActivated += AddEntry;
        if (phone != null) phone.OnOpenChanged += HandlePhoneOpenChanged;

        Refresh();
    }

    private void OnDestroy()
    {
        if (taskManager != null) taskManager.OnTaskActivated -= AddEntry;
        if (phone != null) phone.OnOpenChanged -= HandlePhoneOpenChanged;
    }

    private void Update()
    {
        if (phone != null && !phone.IsOpen) return;

        float scroll = scrollAction.ReadValue<float>();
        if (scroll == 0f) return;

        scrollIndex += scroll > 0f ? -1 : 1;
        Refresh();
    }

    private void HandlePhoneOpenChanged(bool open)
    {
        if (open)
        {
            scrollIndex = 0;
            Refresh();
            return;
        }

        foreach (LocationGroup group in groups)
        {
            foreach (TaskEntryView entry in group.Entries)
                entry.SetNew(false);
        }
    }

    private void AddEntry(TaskLogic task)
    {
        string location = LocationArea.NameAt(task.transform.position, unknownLocation);
        LocationGroup group = GetOrCreateGroup(location);

        TaskEntryView entry = Instantiate(entryTemplate, entryTemplate.transform.parent);
        entry.gameObject.SetActive(true);
        entry.Bind(task, CurrentTime(), RemoveEntry);

        group.Entries.Add(entry);
        Refresh();
    }

    private void RemoveEntry(TaskEntryView entry)
    {
        LocationGroup group = groups.Find(g => g.Entries.Contains(entry));
        if (group != null)
        {
            group.Entries.Remove(entry);
            if (group.Entries.Count == 0)
            {
                Destroy(group.Header.gameObject);
                groups.Remove(group);
            }
        }

        Destroy(entry.gameObject);
        Refresh();
    }

    private static string CurrentTime()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null || gameManager.Clock == null) return string.Empty;

        return gameManager.Clock.TimeString;
    }

    private LocationGroup GetOrCreateGroup(string location)
    {
        LocationGroup group = groups.Find(g => g.Name == location);
        if (group != null) return group;

        TMP_Text header = Instantiate(headerTemplate, headerTemplate.transform.parent);
        header.text = location;
        header.gameObject.SetActive(true);

        group = new LocationGroup { Name = location, Header = header };
        groups.Add(group);
        return group;
    }

    private void Refresh()
    {
        string countHex = ColorUtility.ToHtmlStringRGB(countColor);

        rows.Clear();
        foreach (LocationGroup group in groups)
        {
            group.Header.text = group.Name + "  <color=#" + countHex + ">" + group.Entries.Count + "</color>";
            rows.Add(new Row { Rect = group.Header.rectTransform, Height = headerHeight, X = headerX });

            foreach (TaskEntryView entry in group.Entries)
                rows.Add(new Row { Rect = entry.Rect, Height = entryHeight, X = entryX });
        }

        float available = ((RectTransform)transform).rect.height + top;
        int maxScroll = MaxScrollIndex(available);
        scrollIndex = Mathf.Clamp(scrollIndex, 0, maxScroll);

        float offset = 0f;
        float hiddenAbove = 0f;
        float total = 0f;
        bool full = false;
        for (int i = 0; i < rows.Count; i++)
        {
            Row row = rows[i];
            total += row.Height;
            if (i < scrollIndex) hiddenAbove += row.Height;
            if (i >= scrollIndex && !full && offset + row.Height > available) full = true;

            bool visible = i >= scrollIndex && !full;
            row.Rect.gameObject.SetActive(visible);
            if (!visible) continue;

            row.Rect.anchoredPosition = new Vector2(row.X, top - offset);
            offset += row.Height;
        }

        UpdateScrollBar(maxScroll > 0, hiddenAbove, offset, total);

        if (emptyMessage != null) emptyMessage.SetActive(groups.Count == 0);
    }

    private void UpdateScrollBar(bool scrollable, float hiddenAbove, float shown, float total)
    {
        if (scrollBar != null) scrollBar.SetActive(scrollable);
        if (!scrollable || scrollThumb == null) return;

        float start = hiddenAbove / total;
        float end = (hiddenAbove + shown) / total;
        scrollThumb.anchorMin = new Vector2(0f, 1f - end);
        scrollThumb.anchorMax = new Vector2(1f, 1f - start);
        scrollThumb.offsetMin = Vector2.zero;
        scrollThumb.offsetMax = Vector2.zero;
    }

    private int MaxScrollIndex(float available)
    {
        float used = 0f;
        for (int i = rows.Count - 1; i >= 0; i--)
        {
            used += rows[i].Height;
            if (used > available) return i + 1;
        }

        return 0;
    }
}
