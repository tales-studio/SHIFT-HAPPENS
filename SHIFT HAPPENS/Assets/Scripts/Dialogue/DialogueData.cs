using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Shift Happens/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public string speakerName;
    public Sprite portrait;
    [TextArea(2, 5)] public string[] lines = new string[0];
}
