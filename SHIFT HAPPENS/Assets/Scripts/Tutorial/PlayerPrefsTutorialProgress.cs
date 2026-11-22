using UnityEngine;

public sealed class PlayerPrefsTutorialProgress : ITutorialProgress
{
    private const string SeenKey = "tutorial_seen";

    public bool HasSeen => PlayerPrefs.GetInt(SeenKey, 0) == 1;

    public void MarkSeen()
    {
        PlayerPrefs.SetInt(SeenKey, 1);
        PlayerPrefs.Save();
    }

    public void Reset()
    {
        PlayerPrefs.DeleteKey(SeenKey);
        PlayerPrefs.Save();
    }
}
