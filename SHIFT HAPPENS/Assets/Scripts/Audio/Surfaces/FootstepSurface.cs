using UnityEngine;

[CreateAssetMenu(fileName = "NewFootstepSurface", menuName = "Shift Happens/Audio/Footstep Surface")]
public class FootstepSurface : ScriptableObject
{
    [SerializeField] private SoundData steps;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField, Min(0f)] private float noiseMultiplier = 1f;

    public SoundData Steps => steps;
    public float Volume => volume;
    public float NoiseMultiplier => noiseMultiplier;
}
