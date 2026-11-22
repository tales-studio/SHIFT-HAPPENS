using UnityEngine;

[DisallowMultipleComponent]
public class CutsceneVolume : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private IAudioVolumeService volume;

    private void Start()
    {
        volume = AudioServices.Volume;
        volume.VolumeChanged += Apply;
        Apply();
    }

    private void OnDestroy()
    {
        if (volume != null) volume.VolumeChanged -= Apply;
    }

    private void Apply()
    {
        audioSource.volume = volume.MusicEnabled ? volume.MasterVolume * volume.MusicVolume : 0f;
    }
}
