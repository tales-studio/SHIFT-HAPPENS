using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    [SerializeField] private SoundId sound = SoundId.None;
    [SerializeField] private SoundData soundOverride;

    [SerializeField] private bool positional = true;
    [SerializeField] private bool followThisObject = true;
    [SerializeField] private bool playOnStart;
    [SerializeField, Min(0f)] private float stopFadeOut = 0.3f;

    private SoundHandle handle;
    private bool started;

    private void Start()
    {
        started = true;
        if (playOnStart) Play();
    }

    private void OnEnable()
    {
        if (started && playOnStart) Play();
    }

    private void OnDisable()
    {
        handle.Stop();
    }

    public void Play()
    {
        if (handle.IsLooping) return;

        ISfxService sfx = AudioServices.Sfx;
        if (soundOverride != null)
        {
            if (!positional) handle = sfx.Play(soundOverride);
            else if (followThisObject) handle = sfx.PlayAttached(soundOverride, transform);
            else handle = sfx.PlayAt(soundOverride, transform.position);
        }
        else
        {
            if (!positional) handle = sfx.Play(sound);
            else if (followThisObject) handle = sfx.PlayAttached(sound, transform);
            else handle = sfx.PlayAt(sound, transform.position);
        }
    }

    public void Stop()
    {
        handle.Stop(stopFadeOut);
    }

    public void SetPlaying(bool playing)
    {
        if (playing) Play();
        else Stop();
    }
}
