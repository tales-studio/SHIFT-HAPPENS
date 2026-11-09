using TMPro;
using UnityEngine;

/// <summary>
/// Convierte la ansiedad (0 a 1) en pulsaciones (por defecto de 60 a 180 BPM),
/// las muestra en un texto y hace latir el icono del corazon a ese ritmo.
/// </summary>
public class BPMDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text bpmText;
    [SerializeField] private Transform heartIcon;

    [SerializeField] private float minBpm = 60f;
    [SerializeField] private float maxBpm = 180f;

    [SerializeField] private float smoothSpeed = 3f;

    [SerializeField] private float pulseSize = 0.25f;

    private float displayedBpm;
    private float beatPhase;

    private void Start()
    {
        displayedBpm = minBpm;
    }

    private void Update()
    {
        if (AnxietyManager.Instance == null) return;

        float targetBpm = Mathf.Lerp(minBpm, maxBpm, AnxietyManager.Instance.NormalizedAnxiety);

        displayedBpm = Mathf.Lerp(displayedBpm, targetBpm, smoothSpeed * Time.deltaTime);

        bpmText.text = Mathf.RoundToInt(displayedBpm) + " BPM";
        HeartbeatAnim();
    }

    private void HeartbeatAnim()
    {
        if (heartIcon == null) return;

        beatPhase += (displayedBpm / 60f) * Time.deltaTime;
        beatPhase = beatPhase % 1f;

        float pulse = 1f - Mathf.Clamp01(beatPhase * 3f);
        float scale = 1f + pulse * pulseSize;

        heartIcon.localScale = new Vector3(scale, scale, 1f);
    }
}
