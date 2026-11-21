using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CreditsScreenBuilder
{
    private const string ScreenName = "CreditsScreen";
    private const string ViewportName = "Viewport";
    private const string ContentName = "Content";
    private const float ScrollSensitivity = 20f;
    private const string ScrollbarName = "Scrollbar";
    private const float ScrollbarWidth = 14f;
    private const float ScrollbarGap = 30f;
    private static readonly Color ScrollbarTrackColor = new Color(0.16f, 0.16f, 0.2f, 0.6f);
    private const string FontGuid = "3289301038f08104f9b59b212961d0f8";
    private const string DataFolder = "Assets/ScriptableObjects/Credits";
    private const string DataPath = DataFolder + "/Credits.asset";
    private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";

    private static readonly Vector2 ViewportSize = new Vector2(1400f, 430f);
    private static readonly Vector2 ViewportPosition = new Vector2(0f, -5f);
    private const int EdgeSoftness = 120;

    private static readonly Color HeadingColor = new Color(0.65f, 0.12f, 0.11f, 1f);
    private static readonly Color RoleColor = new Color(0.6f, 0.6f, 0.75f, 1f);
    private static readonly Color NamesColor = Color.white;
    private static readonly Color ClosingColor = Color.white;

    [MenuItem("Shift Happens/UI/Build Credits Screen")]
    private static void Build()
    {
        Transform screen = FindScreen();
        if (screen == null)
        {
            EditorUtility.DisplayDialog("Credits Screen Builder", "Open the MainMenu scene first. No UIScreen named \"" + ScreenName + "\" was found.", "OK");
            return;
        }

        Undo.SetCurrentGroupName("Build Credits Screen");
        int undoGroup = Undo.GetCurrentGroup();

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(FontGuid));

        RectTransform viewport = (RectTransform)screen.Find(ViewportName);
        if (viewport == null) viewport = Register(CreateViewport(screen));

        RectTransform content = (RectTransform)viewport.Find(ContentName);
        if (content == null) content = Register(CreateContent(viewport));

        TMP_Text heading = FindOrCreateTemplate(content, "HeadingTemplate", "SECTION HEADING", font, 56f, HeadingColor, FontStyles.Bold);
        TMP_Text role = FindOrCreateTemplate(content, "RoleTemplate", "Role", font, 30f, RoleColor, FontStyles.Normal);
        TMP_Text names = FindOrCreateTemplate(content, "NamesTemplate", "Name One\nName Two", font, 40f, NamesColor, FontStyles.Normal);
        TMP_Text closing = FindOrCreateTemplate(content, "ClosingTemplate", "Thanks for playing!", font, 64f, ClosingColor, FontStyles.Bold);

        ScrollRect scrollRect = EnsureScrollRect(viewport, content);

        Transform existingScrollbar = screen.Find(ScrollbarName);
        Scrollbar scrollbar = existingScrollbar != null ? existingScrollbar.GetComponent<Scrollbar>() : null;
        if (scrollbar == null) scrollbar = Register(CreateScrollbar(screen, viewport));

        scrollRect.verticalScrollbar = scrollbar;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        InputSystemUIInputModule module = FindInputModule(screen.gameObject.scene);

        CreditsRoll roll = screen.GetComponent<CreditsRoll>();
        if (roll == null) roll = Undo.AddComponent<CreditsRoll>(screen.gameObject);

        SerializedObject so = new SerializedObject(roll);
        so.FindProperty("data").objectReferenceValue = LoadOrCreateData();
        so.FindProperty("viewport").objectReferenceValue = viewport;
        so.FindProperty("content").objectReferenceValue = content;
        so.FindProperty("headingTemplate").objectReferenceValue = heading;
        so.FindProperty("roleTemplate").objectReferenceValue = role;
        so.FindProperty("namesTemplate").objectReferenceValue = names;
        so.FindProperty("closingTemplate").objectReferenceValue = closing;
        so.FindProperty("fastForwardAction").objectReferenceValue = module != null && module.submit != null ? module.submit : FindAction("Submit");
        so.FindProperty("scrollRect").objectReferenceValue = scrollRect;
        so.FindProperty("scrollAction").objectReferenceValue = module != null && module.move != null ? module.move : FindAction("Navigate");
        so.ApplyModifiedProperties();

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(screen.gameObject.scene);
        Selection.activeGameObject = screen.gameObject;
        Debug.Log("CreditsScreenBuilder: built the credits roll under " + ScreenName + ". Edit " + DataPath + " to change the credits, and save the scene to keep the layout.");
    }

    private static RectTransform CreateViewport(Transform screen)
    {
        GameObject go = new GameObject(ViewportName, typeof(RectTransform), typeof(RectMask2D));
        go.layer = screen.gameObject.layer;

        RectTransform rect = (RectTransform)go.transform;
        rect.SetParent(screen, false);
        rect.SetSiblingIndex(BackButtonIndex(screen));
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = ViewportPosition;
        rect.sizeDelta = ViewportSize;

        go.GetComponent<RectMask2D>().softness = new Vector2Int(0, EdgeSoftness);
        return rect;
    }

    private static RectTransform CreateContent(RectTransform viewport)
    {
        GameObject go = new GameObject(ContentName, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        go.layer = viewport.gameObject.layer;

        RectTransform rect = (RectTransform)go.transform;
        rect.SetParent(viewport, false);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout = go.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 0f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = go.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        return rect;
    }

    private static TMP_Text CreateTemplate(Transform parent, string name, string text, TMP_FontAsset font, float size, Color color, FontStyles style)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.horizontalAlignment = HorizontalAlignmentOptions.Center;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static CreditsData LoadOrCreateData()
    {
        string[] guids = AssetDatabase.FindAssets("t:" + nameof(CreditsData));
        if (guids.Length > 0) return AssetDatabase.LoadAssetAtPath<CreditsData>(AssetDatabase.GUIDToAssetPath(guids[0]));

        if (!AssetDatabase.IsValidFolder(DataFolder)) AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Credits");

        CreditsData data = ScriptableObject.CreateInstance<CreditsData>();
        data.sections = new List<CreditsData.Section>
        {
            Section("Development Team",
                Entry("Programming", "Name"),
                Entry("Art", "Name"),
                Entry("Design", "Name"),
                Entry("Audio", "Name")),
            Section("Third-Party Assets",
                Entry("Asset Name", "Author", "License")),
            Section("Special Thanks",
                Entry("", "Name"))
        };

        AssetDatabase.CreateAsset(data, DataPath);
        AssetDatabase.SaveAssets();
        return data;
    }

    private static CreditsData.Section Section(string heading, params CreditsData.Entry[] entries)
    {
        return new CreditsData.Section { heading = heading, entries = new List<CreditsData.Entry>(entries) };
    }

    private static CreditsData.Entry Entry(string role, params string[] names)
    {
        return new CreditsData.Entry { role = role, names = new List<string>(names) };
    }

    private static Scrollbar CreateScrollbar(Transform screen, RectTransform viewport)
    {
        DefaultControls.Resources resources = new DefaultControls.Resources
        {
            standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
            background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd")
        };

        GameObject go = DefaultControls.CreateScrollbar(resources);
        go.name = ScrollbarName;
        SetLayerRecursive(go, screen.gameObject.layer);

        RectTransform rect = (RectTransform)go.transform;
        rect.SetParent(screen, false);
        rect.SetSiblingIndex(viewport.GetSiblingIndex() + 1);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = viewport.anchoredPosition + new Vector2(viewport.sizeDelta.x * 0.5f + ScrollbarGap, 0f);
        rect.sizeDelta = new Vector2(ScrollbarWidth, viewport.sizeDelta.y);

        go.GetComponent<Image>().color = ScrollbarTrackColor;

        Scrollbar scrollbar = go.GetComponent<Scrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scrollbar.navigation = new Navigation { mode = Navigation.Mode.None };
        scrollbar.targetGraphic.color = Color.white;

        ColorBlock colors = scrollbar.colors;
        colors.normalColor = HeadingColor;
        colors.highlightedColor = Color.Lerp(HeadingColor, Color.white, 0.25f);
        colors.pressedColor = Color.Lerp(HeadingColor, Color.black, 0.25f);
        colors.selectedColor = HeadingColor;
        scrollbar.colors = colors;

        go.SetActive(false);
        return scrollbar;
    }

    private static void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform) SetLayerRecursive(child.gameObject, layer);
    }

    private static T Register<T>(T component) where T : Component
    {
        Undo.RegisterCreatedObjectUndo(component.gameObject, "Create " + component.name);
        return component;
    }

    private static TMP_Text FindOrCreateTemplate(RectTransform content, string name, string text, TMP_FontAsset font, float size, Color color, FontStyles style)
    {
        Transform existing = content.Find(name);
        if (existing != null && existing.TryGetComponent(out TMP_Text found)) return found;

        return Register(CreateTemplate(content, name, text, font, size, color, style));
    }

    private static ScrollRect EnsureScrollRect(RectTransform viewport, RectTransform content)
    {
        Image hitArea = viewport.GetComponent<Image>();
        if (hitArea == null)
        {
            hitArea = Undo.AddComponent<Image>(viewport.gameObject);
            hitArea.color = Color.clear;
        }

        ScrollRect scrollRect = viewport.GetComponent<ScrollRect>();
        if (scrollRect == null) scrollRect = Undo.AddComponent<ScrollRect>(viewport.gameObject);

        Undo.RecordObject(scrollRect, "Configure Credits Scroll");
        scrollRect.viewport = viewport;
        scrollRect.content = content;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = true;
        scrollRect.scrollSensitivity = ScrollSensitivity;
        scrollRect.verticalScrollbar = null;
        scrollRect.horizontalScrollbar = null;
        scrollRect.enabled = false;
        return scrollRect;
    }

    private static InputSystemUIInputModule FindInputModule(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            InputSystemUIInputModule module = root.GetComponentInChildren<InputSystemUIInputModule>(true);
            if (module != null) return module;
        }
        return null;
    }

    private static InputActionReference FindAction(string actionName)
    {
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(InputActionsPath))
        {
            if (asset is InputActionReference reference && reference.action != null
                && reference.action.name == actionName && reference.action.actionMap != null && reference.action.actionMap.name == "UI")
                return reference;
        }
        return null;
    }

    private static int BackButtonIndex(Transform screen)
    {
        foreach (BackButton backButton in screen.GetComponentsInChildren<BackButton>(true))
            if (backButton.transform.parent == screen) return backButton.transform.GetSiblingIndex();
        return screen.childCount;
    }

    private static Transform FindScreen()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (UIScreen candidate in root.GetComponentsInChildren<UIScreen>(true))
                    if (candidate.name == ScreenName) return candidate.transform;
            }
        }
        return null;
    }
}
