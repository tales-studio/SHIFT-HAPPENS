using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class OptionsScreenBuilder
{
    private const string ScreenName = "OptionsScreen";
    private const string ContainerName = "VolumeRows";
    private const string FontGuid = "3289301038f08104f9b59b212961d0f8";
    private const string GameplayUIPath = "Assets/Prefabs/UI/GameplayUI.prefab";
    private const string PauseScreenName = "PauseScreen";
    private const string OptionsButtonName = "OptionsButton";
    private const string PauseOptionsNormalPath = "Assets/Art/Interfaces/Pause/Pause_Options_Normal.png";
    private const string PauseOptionsHighlightedPath = "Assets/Art/Interfaces/Pause/Pause_Options_Highlighted.png";
    private static readonly Vector2 OptionsSpriteSize = new Vector2(513f, 91f);
    private const float PauseArtScale = 0.664f;

    private static readonly Color PanelColor = new Color(0.16f, 0.16f, 0.2f, 1f);
    private static readonly Color AccentColor = new Color(0.6f, 0.6f, 0.75f, 1f);
    private static readonly Color TitleRed = new Color(0.65f, 0.12f, 0.11f, 1f);
    private static readonly Color ToggleBoxColor = new Color(0.92f, 0.92f, 0.96f, 1f);

    private const float RowWidth = 1000f;
    private const float RowHeight = 70f;
    private const float RowSpacing = 20f;
    private const float LabelWidth = 240f;
    private const float ToggleSize = 32f;
    private const float SliderWidth = 500f;
    private const float ValueWidth = 120f;

    [MenuItem("Shift Happens/UI/Build Options Volume Rows")]
    private static void Build()
    {
        Transform screen = FindScreen();
        if (screen == null)
        {
            EditorUtility.DisplayDialog("Options Screen Builder", "Open the MainMenu scene first. No GameObject named \"" + ScreenName + "\" was found.", "OK");
            return;
        }

        Undo.SetCurrentGroupName("Build Options Volume Rows");
        int undoGroup = Undo.GetCurrentGroup();

        Transform existing = screen.Find(ContainerName);
        if (existing != null) Undo.DestroyObjectImmediate(existing.gameObject);

        RectTransform container = BuildVolumeRows(screen);

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(screen.gameObject.scene);
        Selection.activeGameObject = container.gameObject;
        Debug.Log("OptionsScreenBuilder: built 4 volume rows under " + ScreenName + ". Save the scene to keep them.");
    }

    [MenuItem("Shift Happens/UI/Build Pause Options Screen")]
    private static void BuildPause()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(GameplayUIPath);
        try
        {
            UIScreen pause = null;
            foreach (UIScreen candidate in root.GetComponentsInChildren<UIScreen>(true))
                if (candidate.name == PauseScreenName) pause = candidate;

            if (pause == null)
            {
                EditorUtility.DisplayDialog("Options Screen Builder", "No \"" + PauseScreenName + "\" was found in " + GameplayUIPath + ".", "OK");
                return;
            }

            Transform pauseRoot = pause.transform;
            Transform parent = pauseRoot.parent;

            Transform oldScreen = parent.Find(ScreenName);
            if (oldScreen != null) Object.DestroyImmediate(oldScreen.gameObject);

            Transform oldButton = pauseRoot.Find(OptionsButtonName);
            bool buttonExisted = oldButton != null;
            if (buttonExisted) Object.DestroyImmediate(oldButton.gameObject);

            TMP_FontAsset font = LoadFont();
            UIScreen optionsScreen = CreatePauseOptionsScreen(parent, pauseRoot, font);
            CreatePauseOptionsButton(pauseRoot, optionsScreen, !buttonExisted);

            PrefabUtility.SaveAsPrefabAsset(root, GameplayUIPath);
            Debug.Log("OptionsScreenBuilder: added the Options button and " + ScreenName + " to " + GameplayUIPath + ".");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static RectTransform BuildVolumeRows(Transform screen)
    {
        TMP_FontAsset font = LoadFont();
        DefaultControls.Resources resources = LoadResources();

        RectTransform container = CreateContainer(screen);

        Slider master = BuildRow(container, "MASTER", VolumeSetting.Channel.Master, font, resources, out Toggle masterToggle);
        Slider music = BuildRow(container, "MUSIC", VolumeSetting.Channel.Music, font, resources, out Toggle musicToggle);
        Slider sfx = BuildRow(container, "SFX", VolumeSetting.Channel.Sfx, font, resources, out Toggle sfxToggle);
        Slider ui = BuildRow(container, "UI", VolumeSetting.Channel.UI, font, resources, out Toggle uiToggle);

        Button back = FindBackButton(screen);

        Selectable[] chain = { masterToggle, master, musicToggle, music, sfxToggle, sfx, uiToggle, ui, back };
        if (back != null && !EditorSceneManager.IsPreviewSceneObject(back)) Undo.RecordObject(back, "Link Back Button");
        LinkChain(chain);

        UIScreen uiScreen = screen.GetComponent<UIScreen>();
        if (uiScreen != null)
        {
            SerializedObject so = new SerializedObject(uiScreen);
            so.FindProperty("firstSelected").objectReferenceValue = master;
            so.ApplyModifiedProperties();
        }

        return container;
    }

    private static TMP_FontAsset LoadFont()
    {
        return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(FontGuid));
    }

    private static UIScreen CreatePauseOptionsScreen(Transform parent, Transform pauseRoot, TMP_FontAsset font)
    {
        GameObject go = new GameObject(ScreenName, typeof(RectTransform), typeof(UIScreen));
        go.layer = pauseRoot.gameObject.layer;
        RectTransform rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.SetSiblingIndex(pauseRoot.GetSiblingIndex() + 1);
        Stretch(rect, Vector2.zero);

        Transform pauseDim = pauseRoot.Find("Dim");
        if (pauseDim != null)
        {
            GameObject dim = Object.Instantiate(pauseDim.gameObject, rect, false);
            dim.name = "Dim";
        }

        TMP_Text title = CreateText(rect, "Title", "OPTIONS", font, 96f, HorizontalAlignmentOptions.Center, 1200f);
        Object.DestroyImmediate(title.GetComponent<LayoutElement>());
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0f, 300f);
        titleRect.sizeDelta = new Vector2(1200f, 160f);

        CreateBackButton(rect, font);
        BuildVolumeRows(rect);

        go.SetActive(false);
        return go.GetComponent<UIScreen>();
    }

    private static void CreateBackButton(Transform screen, TMP_FontAsset font)
    {
        GameObject go = new GameObject("BackButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.layer = screen.gameObject.layer;
        RectTransform rect = (RectTransform)go.transform;
        rect.SetParent(screen, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -280f);
        rect.sizeDelta = new Vector2(420f, 90f);

        Image image = go.GetComponent<Image>();
        image.color = PanelColor;

        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.colors = MenuColors();

        TMP_Text label = CreateText(rect, "Text", "Back", font, 40f, HorizontalAlignmentOptions.Center, 420f);
        Object.DestroyImmediate(label.GetComponent<LayoutElement>());
        Stretch(label.rectTransform, Vector2.zero);

        go.AddComponent<UISoundButton>();
        go.AddComponent<SelectOnHover>();
        go.AddComponent<ButtonFeedback>();
        go.AddComponent<BackButton>();
    }

    private static void CreatePauseOptionsButton(Transform pauseRoot, UIScreen optionsScreen, bool shiftButtonsBelow)
    {
        Transform resume = pauseRoot.Find("ResumeButton");
        Transform restart = pauseRoot.Find("RestartButton");
        Transform mainMenu = pauseRoot.Find("MainMenuButton");
        if (restart == null) return;

        GameObject go = Object.Instantiate(restart.gameObject, pauseRoot, false);
        go.name = OptionsButtonName;
        go.transform.SetSiblingIndex(resume != null ? resume.GetSiblingIndex() + 1 : restart.GetSiblingIndex());

        ReloadSceneButton reload = go.GetComponent<ReloadSceneButton>();
        if (reload != null) Object.DestroyImmediate(reload);

        OpenScreenButton open = go.AddComponent<OpenScreenButton>();
        SerializedObject so = new SerializedObject(open);
        so.FindProperty("target").objectReferenceValue = optionsScreen;
        so.ApplyModifiedPropertiesWithoutUndo();

        Sprite normal = AssetDatabase.LoadAssetAtPath<Sprite>(PauseOptionsNormalPath);
        Sprite highlighted = AssetDatabase.LoadAssetAtPath<Sprite>(PauseOptionsHighlightedPath);

        Button button = go.GetComponent<Button>();
        Image image = (Image)button.targetGraphic;
        if (normal != null) image.sprite = normal;
        if (highlighted != null)
        {
            SpriteState state = button.spriteState;
            state.highlightedSprite = highlighted;
            state.pressedSprite = highlighted;
            state.selectedSprite = highlighted;
            button.spriteState = state;
        }

        TMP_Text label = go.GetComponentInChildren<TMP_Text>(true);
        if (label != null) label.text = "Options";

        RectTransform rect = (RectTransform)go.transform;
        RectTransform restartRect = (RectTransform)restart;
        rect.sizeDelta = new Vector2(OptionsSpriteSize.x * PauseArtScale, OptionsSpriteSize.y * PauseArtScale);
        float left = restartRect.anchoredPosition.x - restartRect.sizeDelta.x * 0.5f;
        rect.anchoredPosition = new Vector2(left + rect.sizeDelta.x * 0.5f, restartRect.anchoredPosition.y);

        if (!shiftButtonsBelow) return;

        float step = resume != null ? ((RectTransform)resume).anchoredPosition.y - restartRect.anchoredPosition.y : 117.6f;
        restartRect.anchoredPosition -= new Vector2(0f, step);
        if (mainMenu != null) ((RectTransform)mainMenu).anchoredPosition -= new Vector2(0f, step);
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

    private static Button FindBackButton(Transform screen)
    {
        foreach (BackButton backButton in screen.GetComponentsInChildren<BackButton>(true))
            if (backButton.transform.parent == screen) return backButton.GetComponent<Button>();
        return null;
    }

    private static DefaultControls.Resources LoadResources()
    {
        return new DefaultControls.Resources
        {
            standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
            background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
            knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
            checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd")
        };
    }

    private static RectTransform CreateContainer(Transform screen)
    {
        GameObject go = new GameObject(ContainerName, typeof(RectTransform), typeof(VerticalLayoutGroup));
        go.layer = screen.gameObject.layer;

        RectTransform rect = (RectTransform)go.transform;
        rect.SetParent(screen, false);
        int backIndex = FindBackButton(screen) != null ? FindBackButton(screen).transform.GetSiblingIndex() : screen.childCount - 1;
        rect.SetSiblingIndex(backIndex);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -10f);
        rect.sizeDelta = new Vector2(RowWidth, RowHeight * 4f + RowSpacing * 3f);

        VerticalLayoutGroup layout = go.GetComponent<VerticalLayoutGroup>();
        layout.spacing = RowSpacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        if (!EditorSceneManager.IsPreviewSceneObject(go)) Undo.RegisterCreatedObjectUndo(go, "Create Volume Rows");
        return rect;
    }

    private static Slider BuildRow(RectTransform container, string label, VolumeSetting.Channel channel, TMP_FontAsset font, DefaultControls.Resources resources, out Toggle toggle)
    {
        GameObject row = new GameObject(Capitalize(label) + "Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.layer = container.gameObject.layer;
        row.transform.SetParent(container, false);
        row.GetComponent<LayoutElement>().preferredHeight = RowHeight;

        HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 25f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        CreateText(row.transform, "Label", label, font, 40f, HorizontalAlignmentOptions.Right, LabelWidth);

        toggle = CreateToggle(row.transform, resources);

        Slider slider = CreateSlider(row.transform, resources);
        TMP_Text value = CreateText(row.transform, "Value", "100%", font, 36f, HorizontalAlignmentOptions.Right, ValueWidth);

        VolumeSetting setting = row.AddComponent<VolumeSetting>();
        SerializedObject so = new SerializedObject(setting);
        so.FindProperty("channel").enumValueIndex = (int)channel;
        so.FindProperty("slider").objectReferenceValue = slider;
        so.FindProperty("toggle").objectReferenceValue = toggle;
        so.FindProperty("valueLabel").objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();

        return slider;
    }

    private static TMP_Text CreateText(Transform parent, string name, string text, TMP_FontAsset font, float size, HorizontalAlignmentOptions alignment, float width)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = Color.white;
        tmp.horizontalAlignment = alignment;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.raycastTarget = false;

        LayoutElement element = go.GetComponent<LayoutElement>();
        element.preferredWidth = width;
        element.preferredHeight = RowHeight;
        return tmp;
    }

    private static Toggle CreateToggle(Transform parent, DefaultControls.Resources resources)
    {
        GameObject go = DefaultControls.CreateToggle(resources);
        go.name = "Toggle";
        SetLayerRecursive(go, parent.gameObject.layer);
        go.transform.SetParent(parent, false);

        Transform legacyLabel = go.transform.Find("Label");
        if (legacyLabel != null) Object.DestroyImmediate(legacyLabel.gameObject);

        LayoutElement element = go.AddComponent<LayoutElement>();
        element.preferredWidth = ToggleSize;
        element.preferredHeight = ToggleSize;

        RectTransform background = (RectTransform)go.transform.Find("Background");
        Stretch(background, Vector2.zero);
        Image backgroundImage = background.GetComponent<Image>();
        backgroundImage.color = ToggleBoxColor;
        Outline border = background.gameObject.AddComponent<Outline>();
        border.effectColor = AccentColor;
        border.effectDistance = new Vector2(2f, -2f);

        RectTransform checkmark = (RectTransform)background.Find("Checkmark");
        Stretch(checkmark, new Vector2(-6f, -6f));
        checkmark.GetComponent<Image>().color = PanelColor;

        Toggle toggle = go.GetComponent<Toggle>();
        toggle.isOn = true;
        toggle.colors = MenuColors();

        AddFeedback(go, true);
        return toggle;
    }

    private static Slider CreateSlider(Transform parent, DefaultControls.Resources resources)
    {
        GameObject go = DefaultControls.CreateSlider(resources);
        go.name = "Slider";
        SetLayerRecursive(go, parent.gameObject.layer);
        go.transform.SetParent(parent, false);

        LayoutElement element = go.AddComponent<LayoutElement>();
        element.preferredWidth = SliderWidth;
        element.preferredHeight = 24f;

        go.transform.Find("Background").GetComponent<Image>().color = PanelColor;
        go.transform.Find("Fill Area/Fill").GetComponent<Image>().color = TitleRed;

        RectTransform handle = (RectTransform)go.transform.Find("Handle Slide Area/Handle");
        handle.sizeDelta = new Vector2(30f, 20f);

        Slider slider = go.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.value = 1f;
        slider.colors = MenuColors();

        SerializedObject so = AddFeedback(go, true);
        so.FindProperty("scaleTarget").objectReferenceValue = handle;
        so.FindProperty("popScale").floatValue = 1.6f;
        so.FindProperty("selectedScale").floatValue = 1.35f;
        so.ApplyModifiedPropertiesWithoutUndo();
        return slider;
    }

    private static SerializedObject AddFeedback(GameObject go, bool outline)
    {
        go.AddComponent<UISoundButton>();
        go.AddComponent<SelectOnHover>();
        ButtonFeedback feedback = go.AddComponent<ButtonFeedback>();
        SerializedObject so = new SerializedObject(feedback);
        so.FindProperty("showOutline").boolValue = outline;
        so.ApplyModifiedPropertiesWithoutUndo();
        return so;
    }

    private static ColorBlock MenuColors()
    {
        ColorBlock colors = ColorBlock.defaultColorBlock;
        colors.normalColor = Color.white;
        colors.highlightedColor = AccentColor;
        colors.pressedColor = new Color(0.45f, 0.45f, 0.55f, 1f);
        colors.selectedColor = AccentColor;
        colors.fadeDuration = 0.1f;
        return colors;
    }

    private static void LinkChain(Selectable[] chain)
    {
        System.Collections.Generic.List<Selectable> items = new System.Collections.Generic.List<Selectable>();
        foreach (Selectable item in chain)
            if (item != null) items.Add(item);

        for (int i = 0; i < items.Count; i++)
        {
            items[i].navigation = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = items[(i - 1 + items.Count) % items.Count],
                selectOnDown = items[(i + 1) % items.Count]
            };
        }
    }

    private static void Stretch(RectTransform rect, Vector2 sizeDelta)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = sizeDelta;
    }

    private static void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform) SetLayerRecursive(child.gameObject, layer);
    }

    private static string Capitalize(string label)
    {
        if (label.Length <= 2) return label;
        return label.Substring(0, 1) + label.Substring(1).ToLowerInvariant();
    }
}
