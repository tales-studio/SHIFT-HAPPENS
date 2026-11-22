using System;
using UnityEngine;

public sealed class PlayerPrefsCutsceneProgress : ICutsceneProgress
{
    private const string SeenKey = "cutscenes_seen";
    private const char Separator = ';';

    public bool HasSeen(Cutscene cutscene)
    {
        if (cutscene == null) return false;

        return Array.IndexOf(ReadSeenIds(), cutscene.Id) >= 0;
    }

    public void MarkSeen(Cutscene cutscene)
    {
        if (cutscene == null || HasSeen(cutscene)) return;

        string seen = PlayerPrefs.GetString(SeenKey, string.Empty);
        string updated = seen.Length == 0 ? cutscene.Id : seen + Separator + cutscene.Id;

        PlayerPrefs.SetString(SeenKey, updated);
        PlayerPrefs.Save();
    }

    public void Reset()
    {
        PlayerPrefs.DeleteKey(SeenKey);
        PlayerPrefs.Save();
    }

    private static string[] ReadSeenIds()
    {
        return PlayerPrefs.GetString(SeenKey, string.Empty).Split(Separator, StringSplitOptions.RemoveEmptyEntries);
    }

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetForEditorPlay()
    {
        PlayerPrefs.DeleteKey(SeenKey);
    }
#endif
}
