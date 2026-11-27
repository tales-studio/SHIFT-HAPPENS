using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Shift Happens/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public string speakerName;
    public Sprite portrait;
    [TextArea(2, 5)] public string[] lines = new string[0];
    [TextArea(2, 5)] public string[] webLines = new string[0];

    public string[] Lines
    {
        get
        {
#if UNITY_WEBGL
            if (webLines != null && webLines.Length > 0) return webLines;
#endif
            return lines;
        }
    }
}
