using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "NewCutscene", menuName = "Shift Happens/Cutscene")]
public class Cutscene : ScriptableObject
{
    [SerializeField] private VideoClip clip;
    [SerializeField] private bool skippable = true;
    [SerializeField, Min(0f)] private float skipHoldDuration = 1f;

    public VideoClip Clip => clip;
    public bool Skippable => skippable;
    public float SkipHoldDuration => skipHoldDuration;
}
