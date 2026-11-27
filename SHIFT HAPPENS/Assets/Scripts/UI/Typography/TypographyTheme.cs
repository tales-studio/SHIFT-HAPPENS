using System;
using TMPro;
using UnityEngine;

public enum TextRole { Title, Body }

[CreateAssetMenu(menuName = "UI/Typography Theme")]
public class TypographyTheme : ScriptableObject
{
    private const string ResourcesPath = "TypographyTheme";

    [Serializable]
    public struct RoleFont
    {
        public TextRole role;
        public TMP_FontAsset font;
    }

    [SerializeField] private RoleFont[] fonts;

    private static TypographyTheme cachedDefault;

    public static TypographyTheme Default
    {
        get
        {
            if (cachedDefault == null) cachedDefault = Resources.Load<TypographyTheme>(ResourcesPath);
            return cachedDefault;
        }
    }

    public bool TryGetFont(TextRole role, out TMP_FontAsset font)
    {
        if (fonts != null)
        {
            foreach (RoleFont entry in fonts)
            {
                if (entry.role == role && entry.font != null)
                {
                    font = entry.font;
                    return true;
                }
            }
        }

        font = null;
        return false;
    }
}
