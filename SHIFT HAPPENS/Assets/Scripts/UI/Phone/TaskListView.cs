using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class TaskListView : MonoBehaviour
{
    private class LocationGroup
    {
        public string Name;
        public TMP_Text Header;
        public readonly List<TaskEntryView> Entries = new List<TaskEntryView>();
    }

    [SerializeField] private PhoneView phone;
    [SerializeField] private TMP_Text headerTemplate;
    [SerializeField] private TaskEntryView entryTemplate;
    [SerializeField] private GameObject emptyMessage;
    [SerializeField] private string unknownLocation = "Elsewhere";
    [SerializeField] private Color countColor = new Color(0.5f, 0.53f, 0.66f);
    [SerializeField, Min(0f)] private float headerHeight = 44f;
    [SerializeField, Min(0f)] private float entryHeight = 86f;

    private readonly List<LocationGroup> groups = new List<LocationGroup>();
    private TaskManager taskManager;
    private float top;
    private float headerX;
    private float entryX;

    private void Awake()
    {
        top = headerTemplate.rectTransform.anchoredPosition.y;
        headerX = headerTemplate.rectTransform.anchoredPosition.x;
        entryX = entryTemplate.Rect.anchoredPosition.x;

        headerTemplate.gameObject.SetActive(false);
        entryTemplate.gameObject.SetActive(false);
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

    private void HandlePhoneOpenChanged(bool open)
    {
        if (open) return;

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
        float offset = 0f;

        string countHex = ColorUtility.ToHtmlStringRGB(countColor);

        foreach (LocationGroup group in groups)
        {
            group.Header.text = group.Name + "  <color=#" + countHex + ">" + group.Entries.Count + "</color>";
            group.Header.rectTransform.anchoredPosition = new Vector2(headerX, top - offset);
            offset += headerHeight;

            foreach (TaskEntryView entry in group.Entries)
            {
                entry.Rect.anchoredPosition = new Vector2(entryX, top - offset);
                offset += entryHeight;
            }
        }

        if (emptyMessage != null) emptyMessage.SetActive(groups.Count == 0);
    }
}
