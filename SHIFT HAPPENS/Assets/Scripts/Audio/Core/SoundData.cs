using UnityEngine;

[CreateAssetMenu(fileName = "NewSound", menuName = "Shift Happens/Audio/Sound Data")]
public class SoundData : ScriptableObject
{
    [Header("Clips")]
    [SerializeField] private SoundCategory category = SoundCategory.Environment;
    [SerializeField] private AudioClip[] clips = new AudioClip[0];
    [SerializeField] private AudioClip[] distortedClips = new AudioClip[0];
    [SerializeField] private bool distortDuringOutbreak = true;

    [Header("Playback")]
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField] private bool loop;

    [Header("Random pitch")]
    [SerializeField] private bool randomizePitch = true;
    [SerializeField, Range(0.1f, 3f)] private float minPitch = 0.92f;
    [SerializeField, Range(0.1f, 3f)] private float maxPitch = 1.08f;

    [Header("Spatial (top-down world sound)")]
    [SerializeField, Range(0f, 1f)] private float spatialBlend = 1f;
    [SerializeField, Min(0.01f)] private float minDistance = 1f;
    [SerializeField, Min(0.01f)] private float maxDistance = 15f;

    [Header("Spam control")]
    [SerializeField, Min(0f)] private float cooldown;
    [SerializeField, Min(0)] private int maxSimultaneous;
    [SerializeField] private bool avoidImmediateRepeat = true;

    [System.NonSerialized] private int lastClipIndex = -1;

    public SoundCategory Category => category;
    public AudioBus Bus => category.ToBus();
    public float Volume => volume;
    public bool Loop => loop;
    public float SpatialBlend => spatialBlend;
    public float MinDistance => minDistance;
    public float MaxDistance => maxDistance;
    public float Cooldown => cooldown;
    public int MaxSimultaneous => maxSimultaneous;
    public bool DistortDuringOutbreak => distortDuringOutbreak;
    public bool HasDistortedClips => distortedClips != null && distortedClips.Length > 0;

    public AudioClip PickClip(bool distorted)
    {
        AudioClip[] pool = distorted && HasDistortedClips ? distortedClips : clips;
        if (pool == null || pool.Length == 0) return null;
        if (pool.Length == 1) return pool[0];

        int index;
        if (avoidImmediateRepeat && lastClipIndex >= 0 && lastClipIndex < pool.Length)
        {
            index = Random.Range(0, pool.Length - 1);
            if (index >= lastClipIndex) index++;
        }
        else
        {
            index = Random.Range(0, pool.Length);
        }

        lastClipIndex = index;
        return pool[index];
    }

    public float PickPitch(bool allowVariation)
    {
        if (allowVariation && randomizePitch)
            return Random.Range(minPitch, maxPitch);
        return randomizePitch ? (minPitch + maxPitch) * 0.5f : minPitch;
    }

    private void OnValidate()
    {
        if (maxPitch < minPitch) maxPitch = minPitch;
        if (maxDistance < minDistance) maxDistance = minDistance;
    }

#if UNITY_EDITOR
    public void EditorSetDefaults(SoundCategory category, bool loop, float spatialBlend, bool randomizePitch, float minPitch, float maxPitch)
    {
        this.category = category;
        this.loop = loop;
        this.spatialBlend = spatialBlend;
        this.randomizePitch = randomizePitch;
        this.minPitch = minPitch;
        this.maxPitch = maxPitch;
    }
#endif
}
