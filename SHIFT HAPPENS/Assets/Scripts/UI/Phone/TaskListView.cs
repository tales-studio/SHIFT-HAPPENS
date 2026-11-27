using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class TaskListView : MonoBehaviour
{
    private class FloorGroup
    {
        public bool HasFloor;
        public BuildingFloor Floor;
        public string Name;
        public TMP_Text Header;
        public RectTransform LeftDivider;
        public RectTransform RightDivider;
        public readonly List<RoomGroup> Rooms = new List<RoomGroup>();
    }

    private class RoomGroup
    {
        public string Name;
        public TMP_Text Header;
        public RectTransform Badge;
        public TMP_Text BadgeCount;
        public readonly List<TaskEntryView> Entries = new List<TaskEntryView>();
    }

    private struct Row
    {
        public RectTransform Rect;
        public float Height;
        public float X;
        public float Gap;
    }

    [SerializeField] private PhoneView phone;
    [SerializeField] private TMP_Text headerTemplate;
    [SerializeField] private TaskEntryView entryTemplate;
    [SerializeField] private GameObject emptyMessage;
    [SerializeField] private FloorCatalog floorCatalog;
    [SerializeField] private string unknownLocation = "Elsewhere";
    [SerializeField] private Sprite countBadgeSprite;
    [SerializeField] private Color countBadgeColor = new Color(1f, 1f, 1f);
    [SerializeField] private Color countTextColor = new Color(0.18f, 0.173f, 0.212f);
    [SerializeField, Min(1f)] private float countBadgeSize = 28f;
    [SerializeField, Min(1f)] private float countFontSize = 18f;
    [SerializeField, Min(0f)] private float countBadgeSpacing = 10f;
    [SerializeField] private Color roomHeaderColor = new Color(1f, 1f, 1f);
    [SerializeField] private Color floorHeaderColor = new Color(0.8f, 0.22f, 0.2f);
    [SerializeField, Min(1f)] private float floorHeaderFontSize = 23f;
    [SerializeField] private float floorHeaderSpacing = 10f;
    [SerializeField] private Color dividerColor = new Color(0.8f, 0.22f, 0.2f, 1f);
    [SerializeField, Min(1f)] private float dividerThickness = 2f;
    [SerializeField, Min(0f)] private float dividerSpacing = 14f;
    [SerializeField, Min(0f)] private float floorHeaderHeight = 46f;
    [SerializeField, Min(0f)] private float floorGap = 20f;
    [SerializeField, Min(0f)] private float headerHeight = 44f;
    [SerializeField, Min(0f)] private float entryHeight = 86f;
    [SerializeField] private GameObject scrollBar;
    [SerializeField] private RectTransform scrollThumb;
    [SerializeField] private InputActionReference scrollAction;

    private readonly List<FloorGroup> floors = new List<FloorGroup>();
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
        scrollAction.action.Enable();
    }

    private void OnDisable()
    {
        scrollAction.action.Disable();
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

        float scroll = scrollAction.action.ReadValue<float>();
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

        foreach (FloorGroup floor in floors)
        {
            foreach (RoomGroup room in floor.Rooms)
            {
                foreach (TaskEntryView entry in room.Entries)
                    entry.SetNew(false);
            }
        }
    }

    private void AddEntry(TaskLogic task)
    {
        LocationArea area = LocationArea.At(task.transform.position);
        FloorGroup floor = GetOrCreateFloor(area);
        RoomGroup room = GetOrCreateRoom(floor, RoomNameFor(area, floor));

        TaskEntryView entry = Instantiate(entryTemplate, entryTemplate.transform.parent);
        entry.gameObject.SetActive(true);
        entry.Bind(task, CurrentTime(), RemoveEntry);

        room.Entries.Add(entry);
        Refresh();
    }

    private void RemoveEntry(TaskEntryView entry)
    {
        foreach (FloorGroup floor in floors)
        {
            RoomGroup room = floor.Rooms.Find(r => r.Entries.Contains(entry));
            if (room == null) continue;

            room.Entries.Remove(entry);
            if (room.Entries.Count == 0)
            {
                if (room.Header != null) Destroy(room.Header.gameObject);
                floor.Rooms.Remove(room);
            }

            if (floor.Rooms.Count == 0)
            {
                Destroy(floor.Header.gameObject);
                floors.Remove(floor);
            }
            break;
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

    private string FloorName(BuildingFloor floor)
    {
        return floorCatalog != null ? floorCatalog.GetName(floor) : floor.ToString();
    }

    private static string RoomNameFor(LocationArea area, FloorGroup floor)
    {
        if (area == null || string.IsNullOrEmpty(area.DisplayName) || area.DisplayName == floor.Name) return null;

        return area.DisplayName;
    }

    private FloorGroup GetOrCreateFloor(LocationArea area)
    {
        bool hasFloor = area != null;
        BuildingFloor floorValue = hasFloor ? area.Floor : default;

        FloorGroup floor = floors.Find(f => f.HasFloor == hasFloor && (!hasFloor || f.Floor == floorValue));
        if (floor != null) return floor;

        floor = new FloorGroup
        {
            HasFloor = hasFloor,
            Floor = floorValue,
            Name = hasFloor ? FloorName(floorValue) : unknownLocation,
            Header = CreateFloorHeader()
        };
        floor.LeftDivider = CreateDivider(floor.Header.rectTransform, 0f, 0.5f);
        floor.RightDivider = CreateDivider(floor.Header.rectTransform, 0.5f, 1f);

        int index = floors.FindIndex(f => !f.HasFloor || (hasFloor && f.Floor > floorValue));
        if (index < 0) floors.Add(floor);
        else floors.Insert(index, floor);

        return floor;
    }

    private static RoomGroup GetOrCreateRoom(FloorGroup floor, string roomName)
    {
        RoomGroup room = floor.Rooms.Find(r => r.Name == roomName);
        if (room != null) return room;

        room = new RoomGroup { Name = roomName };

        if (roomName == null) floor.Rooms.Insert(0, room);
        else floor.Rooms.Add(room);

        return room;
    }

    private TMP_Text CreateHeader()
    {
        TMP_Text header = Instantiate(headerTemplate, headerTemplate.transform.parent);
        header.gameObject.SetActive(true);
        return header;
    }

    private void CreateRoomHeader(RoomGroup room)
    {
        TMP_Text header = CreateHeader();
        header.color = roomHeaderColor;
        room.Header = header;

        GameObject badge = new GameObject("CountBadge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform badgeRect = (RectTransform)badge.transform;
        badgeRect.SetParent(header.rectTransform, false);
        badgeRect.anchorMin = new Vector2(0f, 0.5f);
        badgeRect.anchorMax = new Vector2(0f, 0.5f);
        badgeRect.pivot = new Vector2(0f, 0.5f);
        badgeRect.sizeDelta = new Vector2(countBadgeSize, countBadgeSize);

        Image image = badge.GetComponent<Image>();
        image.sprite = countBadgeSprite;
        image.color = countBadgeColor;
        image.preserveAspect = true;
        image.raycastTarget = false;

        GameObject countObject = new GameObject("Count", typeof(RectTransform));
        RectTransform countRect = (RectTransform)countObject.transform;
        countRect.SetParent(badgeRect, false);
        countRect.anchorMin = Vector2.zero;
        countRect.anchorMax = Vector2.one;
        countRect.offsetMin = Vector2.zero;
        countRect.offsetMax = Vector2.zero;

        TextMeshProUGUI count = countObject.AddComponent<TextMeshProUGUI>();
        count.font = header.font;
        count.fontSharedMaterial = header.fontSharedMaterial;
        count.fontSize = countFontSize;
        count.fontStyle = FontStyles.Bold;
        count.color = countTextColor;
        count.alignment = TextAlignmentOptions.Center;
        count.textWrappingMode = TextWrappingModes.NoWrap;
        count.raycastTarget = false;

        room.Badge = badgeRect;
        room.BadgeCount = count;
    }

    private void PlaceCountBadge(RoomGroup room)
    {
        room.BadgeCount.text = room.Entries.Count.ToString();
        room.Badge.anchoredPosition = new Vector2(room.Header.preferredWidth + countBadgeSpacing, 0f);
    }

    private TMP_Text CreateFloorHeader()
    {
        TMP_Text header = CreateHeader();
        header.color = floorHeaderColor;
        header.fontStyle = FontStyles.UpperCase | FontStyles.Bold;
        header.enableAutoSizing = false;
        header.fontSize = floorHeaderFontSize;
        header.characterSpacing = floorHeaderSpacing;
        header.horizontalAlignment = HorizontalAlignmentOptions.Center;
        return header;
    }

    private RectTransform CreateDivider(RectTransform parent, float anchorMinX, float anchorMaxX)
    {
        GameObject divider = new GameObject("Divider", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

        RectTransform rect = (RectTransform)divider.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(anchorMinX, 0.5f);
        rect.anchorMax = new Vector2(anchorMaxX, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        Image image = divider.GetComponent<Image>();
        image.color = dividerColor;
        image.raycastTarget = false;

        return rect;
    }

    private void PlaceDividers(FloorGroup floor)
    {
        float halfGap = floor.Header.preferredWidth * 0.5f + dividerSpacing;
        float halfThickness = dividerThickness * 0.5f;

        floor.LeftDivider.offsetMin = new Vector2(0f, -halfThickness);
        floor.LeftDivider.offsetMax = new Vector2(-halfGap, halfThickness);

        floor.RightDivider.offsetMin = new Vector2(halfGap, -halfThickness);
        floor.RightDivider.offsetMax = new Vector2(0f, halfThickness);
    }

    private void Refresh()
    {
        rows.Clear();
        foreach (FloorGroup floor in floors)
        {
            floor.Header.text = floor.Name;
            PlaceDividers(floor);
            rows.Add(new Row { Rect = floor.Header.rectTransform, Height = floorHeaderHeight, X = headerX, Gap = floorGap });

            foreach (RoomGroup room in floor.Rooms)
            {
                if (room.Name != null)
                {
                    if (room.Header == null) CreateRoomHeader(room);
                    room.Header.text = room.Name;
                    PlaceCountBadge(room);
                    rows.Add(new Row { Rect = room.Header.rectTransform, Height = headerHeight, X = headerX });
                }

                foreach (TaskEntryView entry in room.Entries)
                    rows.Add(new Row { Rect = entry.Rect, Height = entryHeight, X = entryX });
            }
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
            float gap = i > scrollIndex ? row.Gap : 0f;
            total += row.Height + (i > 0 ? row.Gap : 0f);
            if (i < scrollIndex) hiddenAbove += row.Height + (i > 0 ? row.Gap : 0f);
            if (i >= scrollIndex && !full && offset + gap + row.Height > available) full = true;

            bool visible = i >= scrollIndex && !full;
            row.Rect.gameObject.SetActive(visible);
            if (!visible) continue;

            offset += gap;
            row.Rect.anchoredPosition = new Vector2(row.X, top - offset);
            offset += row.Height;
        }

        UpdateScrollBar(maxScroll > 0, hiddenAbove, offset, total);

        if (emptyMessage != null) emptyMessage.SetActive(floors.Count == 0);
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
            used += rows[i].Gap;
        }

        return 0;
    }
}
