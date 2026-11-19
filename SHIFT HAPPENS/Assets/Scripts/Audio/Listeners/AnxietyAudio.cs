using UnityEngine;

public class AnxietyAudio : MonoBehaviour
{
    [Header("Heartbeat")]
    [SerializeField] private SoundId heartbeat = SoundId.Heartbeat;
    [SerializeField] private BPMDisplay bpmDisplay;
    [SerializeField] private float minBpm = 60f;
    [SerializeField] private float maxBpm = 180f;
    [SerializeField, Range(0f, 1f)] private float heartbeatAudibleFrom = 0.25f;

    [Header("Breathing")]
    [SerializeField] private SoundId breathing = SoundId.PlayerBreathing;
    [SerializeField, Range(0f, 1f)] private float breathingAudibleFrom = 0.4f;
    [SerializeField, Min(0f)] private float breathingFadeOut = 1f;

    [Header("Critical anxiety")]
    [SerializeField] private SoundId critical = SoundId.AnxietyCritical;
    [SerializeField, Range(0f, 1f)] private float criticalThreshold = 0.85f;

    [Header("Response")]
    [SerializeField, Min(0.1f)] private float smoothSpeed = 3f;

    private float anxiety;
    private float beatPhase;
    private bool criticalPlayed;
    private SoundHandle breathingHandle;

    private void Start()
    {
        if (bpmDisplay == null) bpmDisplay = FindFirstObjectByType<BPMDisplay>();
        if (bpmDisplay != null) bpmDisplay.Beat += PlayHeartbeat;
    }

    private void OnDestroy()
    {
        if (bpmDisplay != null) bpmDisplay.Beat -= PlayHeartbeat;
        breathingHandle.Stop();
    }

    private void Update()
    {
        if (!IsActive())
        {
            anxiety = 0f;
            criticalPlayed = false;
            breathingHandle.Stop(breathingFadeOut);
            return;
        }

        anxiety = Mathf.Lerp(anxiety, AnxietyManager.Instance.NormalizedAnxiety, smoothSpeed * Time.deltaTime);

        if (bpmDisplay == null) TickInternalHeartbeat();
        UpdateBreathing();
        UpdateCritical();
    }

    private static bool IsActive()
    {
        return AnxietyManager.Instance != null &&
               GameManager.Instance != null &&
               GameManager.Instance.isPlaying;
    }

    private void TickInternalHeartbeat()
    {
        float bpm = Mathf.Lerp(minBpm, maxBpm, anxiety);
        beatPhase += bpm / 60f * Time.deltaTime;
        if (beatPhase < 1f) return;
        beatPhase %= 1f;
        PlayHeartbeat();
    }

    private void PlayHeartbeat()
    {
        if (!IsActive() || anxiety < heartbeatAudibleFrom) return;
        SoundHandle handle = AudioServices.Sfx.Play(heartbeat);
        handle.SetVolume(Mathf.Lerp(0.3f, 1f, Mathf.InverseLerp(heartbeatAudibleFrom, 1f, anxiety)));
    }

    private void UpdateBreathing()
    {
        if (anxiety < breathingAudibleFrom)
        {
            breathingHandle.Stop(breathingFadeOut);
            return;
        }

        if (!breathingHandle.IsPlaying)
            breathingHandle = AudioServices.Sfx.Play(breathing);

        breathingHandle.SetVolume(Mathf.InverseLerp(breathingAudibleFrom, 1f, anxiety));
    }

    private void UpdateCritical()
    {
        if (!criticalPlayed && anxiety >= criticalThreshold)
        {
            criticalPlayed = true;
            AudioServices.Sfx.Play(critical);
        }
        else if (criticalPlayed && anxiety < criticalThreshold - 0.1f)
        {
            criticalPlayed = false;
        }
    }
}
