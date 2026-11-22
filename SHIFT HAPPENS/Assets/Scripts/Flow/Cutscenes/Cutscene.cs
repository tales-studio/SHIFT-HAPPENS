using UnityEngine;

[CreateAssetMenu(fileName = "NewCutscene", menuName = "Shift Happens/Cutscene")]
public class Cutscene : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string streamingFile;
    [SerializeField] private bool skippable = true;
    [SerializeField, Min(0f)] private float skipHoldDuration = 1f;

    public string Id => string.IsNullOrEmpty(id) ? name : id;
    public bool HasVideo => !string.IsNullOrEmpty(streamingFile);
    public string Url => Application.streamingAssetsPath + "/" + streamingFile;
    public bool Skippable => skippable;
    public float SkipHoldDuration => skipHoldDuration;
}
