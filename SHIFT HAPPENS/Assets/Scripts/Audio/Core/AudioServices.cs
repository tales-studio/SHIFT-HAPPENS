using UnityEngine;

public static class AudioServices
{
    private static ISfxService sfx;
    private static IMusicService music;
    private static IAudioVolumeService volume;

    public static ISfxService Sfx => sfx ?? NullAudioService.Instance;
    public static IMusicService Music => music ?? NullAudioService.Instance;
    public static IAudioVolumeService Volume => volume ?? NullAudioService.Instance;

    public static bool IsAvailable => sfx != null;

    public static void Register(ISfxService sfxService, IMusicService musicService, IAudioVolumeService volumeService)
    {
        sfx = sfxService;
        music = musicService;
        volume = volumeService;
    }

    public static void Unregister(object owner)
    {
        if (ReferenceEquals(sfx, owner)) sfx = null;
        if (ReferenceEquals(music, owner)) music = null;
        if (ReferenceEquals(volume, owner)) volume = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        sfx = null;
        music = null;
        volume = null;
    }
}
