using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

public static class FontStaticBaker
{
    private const string MenuPath = "Assets/Shift Happens/Make Font Static";
    private const string ExtraCharacters = "‘’“”…–—€";

    [MenuItem(MenuPath)]
    private static void MakeStatic()
    {
        TMP_FontAsset font = Selection.activeObject as TMP_FontAsset;

        if (font.atlasPopulationMode == AtlasPopulationMode.Static)
        {
            Debug.Log("FontStaticBaker: " + font.name + " is already Static.");
            return;
        }

        bool allAdded = font.TryAddCharacters(BuildCharacterSet(), out string missing, true);

        font.atlasPopulationMode = AtlasPopulationMode.Static;
        EditorUtility.SetDirty(font);
        AssetDatabase.SaveAssets();

        if (allAdded || string.IsNullOrEmpty(missing))
            Debug.Log("FontStaticBaker: " + font.name + " is now Static with " + font.characterTable.Count + " characters.");
        else
            Debug.LogWarning("FontStaticBaker: " + font.name + " is now Static with " + font.characterTable.Count + " characters. Not added (not in the font or the atlas is full): " + missing);
    }

    [MenuItem(MenuPath, true)]
    private static bool CanMakeStatic()
    {
        return Selection.activeObject is TMP_FontAsset;
    }

    private static string BuildCharacterSet()
    {
        StringBuilder characters = new StringBuilder();
        for (int code = 32; code <= 126; code++) characters.Append((char)code);
        for (int code = 160; code <= 255; code++) characters.Append((char)code);
        characters.Append(ExtraCharacters);
        return characters.ToString();
    }
}
