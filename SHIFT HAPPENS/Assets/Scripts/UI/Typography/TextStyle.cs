using TMPro;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class TextStyle : MonoBehaviour
{
    [SerializeField] private TextRole role = TextRole.Body;
    [SerializeField] private TypographyTheme theme;

    private TypographyTheme Theme => theme != null ? theme : TypographyTheme.Default;

    private void OnEnable() => Apply();

#if UNITY_EDITOR
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null) Apply();
        };
    }
#endif

    public void SetRole(TextRole newRole)
    {
        role = newRole;
        Apply();
    }

    public void Apply()
    {
        TypographyTheme current = Theme;
        if (current == null || !current.TryGetFont(role, out TMP_FontAsset font)) return;

        TMP_Text text = GetComponent<TMP_Text>();
        if (text.font == font) return;

        text.font = font;
        text.fontSharedMaterial = font.material;
    }
}
