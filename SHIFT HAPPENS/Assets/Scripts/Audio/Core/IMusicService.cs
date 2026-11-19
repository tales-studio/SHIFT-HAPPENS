public interface IMusicService
{
    void PlayMusic(SoundId id, float fade = -1f);
    void PlayMusic(SoundData sound, float fade = -1f);

    void StopMusic(float fade = -1f);
}
