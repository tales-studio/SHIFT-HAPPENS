using UnityEngine;

public interface ISfxService
{
    bool Distorted { get; set; }

    SoundHandle Play(SoundId id);
    SoundHandle Play(SoundData sound);

    SoundHandle PlayAt(SoundId id, Vector3 position);
    SoundHandle PlayAt(SoundData sound, Vector3 position);

    SoundHandle PlayAttached(SoundId id, Transform target);
    SoundHandle PlayAttached(SoundData sound, Transform target);

    void StopAll(float fadeOut = 0f);
}
