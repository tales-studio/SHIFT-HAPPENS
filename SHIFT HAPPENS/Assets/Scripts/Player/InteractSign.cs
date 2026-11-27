using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class InteractSign : MonoBehaviour
{
    [SerializeField] private string fallbackText = "E";
    [SerializeField] private TextRole role = TextRole.Title;
    [SerializeField] private float fontSize = 8f;
    [SerializeField] private Color color = Color.white;
    [SerializeField] private Material material;
    [SerializeField] private float heightOffset = 0.6f;
    [SerializeField] private int sortingOrder = 500;
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.75f);
    [SerializeField] private Vector2 backgroundPadding = new Vector2(0.3f, 0.18f);

    private TextMeshPro sign;
    private Transform background;
    private PlayerInputReader input;
    private InputPrompts prompts;

    private void Awake()
    {
        input = GetComponent<PlayerInputReader>();
        CreateSign();
    }

    private void OnEnable()
    {
        prompts = InputPrompts.Instance;
        if (prompts != null) prompts.Changed += RefreshText;
        RefreshText();
    }

    private void OnDisable()
    {
        if (prompts != null) prompts.Changed -= RefreshText;
        Hide();
    }

    private void OnDestroy()
    {
        if (sign != null) Destroy(sign.gameObject);
    }

    public void Show(Vector3 anchor)
    {
        if (sign == null) return;

        sign.transform.position = anchor + Vector3.up * heightOffset;
        sign.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (sign != null) sign.gameObject.SetActive(false);
    }

    private void CreateSign()
    {
        GameObject signObject = new GameObject("InteractSign");
        sign = signObject.AddComponent<TextMeshPro>();
        signObject.AddComponent<TextStyle>().SetRole(role);
        sign.text = fallbackText;
        sign.fontSize = fontSize;
        sign.fontStyle = FontStyles.Bold;
        sign.color = color;
        sign.alignment = TextAlignmentOptions.Bottom;
        sign.textWrappingMode = TextWrappingModes.NoWrap;
        sign.rectTransform.sizeDelta = new Vector2(2f, 2f);
        sign.rectTransform.pivot = new Vector2(0.5f, 0f);
        if (material != null) sign.fontSharedMaterial = material;
        sign.sortingOrder = sortingOrder;
        CreateBackground();
        signObject.SetActive(false);
    }

    private void CreateBackground()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);

        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(sign.transform, false);

        SpriteRenderer backgroundRenderer = backgroundObject.AddComponent<SpriteRenderer>();
        backgroundRenderer.sprite = sprite;
        backgroundRenderer.color = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, Mathf.Min(backgroundColor.a, 0.9f));
        backgroundRenderer.sortingOrder = sortingOrder - 1;

        background = backgroundObject.transform;
        FitBackground();
    }

    private void RefreshText()
    {
        if (sign == null) return;

        string label = prompts != null && input != null ? prompts.GetLabel(input.InteractAction) : null;
        sign.text = string.IsNullOrEmpty(label) ? fallbackText : label;
        FitBackground();
    }

    private void FitBackground()
    {
        if (background == null) return;

        sign.ForceMeshUpdate(true);
        Bounds textBounds = sign.textBounds;
        background.localPosition = new Vector3(textBounds.center.x, textBounds.center.y, 0f);
        background.localScale = new Vector3(
            textBounds.size.x + backgroundPadding.x * 2f,
            textBounds.size.y + backgroundPadding.y * 2f,
            1f);
    }
}
