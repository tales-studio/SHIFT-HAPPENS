using UnityEngine;

public class TaskSoundCue : MonoBehaviour
{
    [SerializeField] private TaskLogic task;
    [SerializeField] private SoundId sound = SoundId.None;
    [SerializeField, Min(0f)] private float repeatInterval = 0f;
    [SerializeField, Min(0f)] private float stopFadeOut = 0.3f;

    private SoundHandle handle;
    private float repeatTimer;

    private void Reset()
    {
        task = GetComponent<TaskLogic>();
    }

    private void OnEnable()
    {
        if (task == null)
        {
            Debug.LogWarning($"{name}: TaskSoundCue is missing its Task reference.", this);
            return;
        }

        task.ActiveChanged += HandleActiveChanged;
        if (task.isActive) Play();
    }

    private void OnDisable()
    {
        if (task != null)
            task.ActiveChanged -= HandleActiveChanged;

        StopCue();
    }

    private void Update()
    {
        if (task == null || !task.isActive || repeatInterval <= 0f) return;
        if (handle.IsPlaying) return;

        repeatTimer -= Time.deltaTime;
        if (repeatTimer <= 0f) Play();
    }

    private void HandleActiveChanged(bool active)
    {
        if (active) Play();
        else StopCue();
    }

    private void Play()
    {
        if (sound == SoundId.None) return;

        handle.Stop();
        handle = AudioServices.Sfx.PlayAttached(sound, transform);
        repeatTimer = repeatInterval;
    }

    private void StopCue()
    {
        handle.Stop(stopFadeOut);
        handle = SoundHandle.None;
    }
}
