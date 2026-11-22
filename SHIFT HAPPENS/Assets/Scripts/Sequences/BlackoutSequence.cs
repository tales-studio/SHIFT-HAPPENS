using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[DisallowMultipleComponent]
public class BlackoutSequence : MonoBehaviour
{
    [Header("Lights")]
    [SerializeField] private Light2D[] lights;
    [SerializeField, Range(0f, 1f)] private float darkIntensity = 0.25f;
    [SerializeField] private Color hostileColor = new Color(0.7f, 0.75f, 1f);

    [Header("Flicker")]
    [SerializeField, Min(0)] private int flickerCount = 4;
    [SerializeField, Min(0f)] private float flickerInterval = 0.12f;
    [SerializeField] private SoundId flickerSound = SoundId.LightFlicker;

    [Header("Darkness")]
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.3f;
    [SerializeField, Min(0f)] private float holdDuration = 2.5f;
    [SerializeField, Min(0f)] private float fadeInDuration = 3f;
    [SerializeField] private SoundId darknessSound = SoundId.NPCStudentsScream;

    [Header("Wake up")]
    [SerializeField] private Transform wakeUpPoint;

    private GameManager gameManager;
    private PlayerMovement lockedMovement;
    private float[] originalIntensities;
    private Coroutine routine;

    private void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager != null) gameManager.OnBlackout += HandleBlackout;
    }

    private void OnDestroy()
    {
        if (gameManager != null) gameManager.OnBlackout -= HandleBlackout;
    }

    private void OnDisable()
    {
        if (routine == null) return;

        StopCoroutine(routine);
        routine = null;
        UnlockPlayer();
    }

    private void HandleBlackout()
    {
        if (routine != null) return;

        routine = StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        LockPlayer();
        SaveLights();

        yield return Flicker();
        SetLights(darkIntensity, hostileColor);
        AudioServices.Sfx.Play(darknessSound);

        IScreenFader fader = SceneServices.Fader;

        yield return fader.FadeOut(fadeOutDuration);
        yield return new WaitForSeconds(holdDuration);

        if (wakeUpPoint != null && Player.Instance != null)
            Player.Instance.Teleporter.PlaceAt(wakeUpPoint.position);

        yield return fader.FadeIn(fadeInDuration);

        UnlockPlayer();
        routine = null;
        gameManager.StartOutbreak();
    }

    private IEnumerator Flicker()
    {
        for (int i = 0; i < flickerCount; i++)
        {
            SetLights(0f, null);
            AudioServices.Sfx.Play(flickerSound);
            yield return new WaitForSeconds(flickerInterval * Random.Range(0.5f, 1.5f));

            SetLights(1f, null);
            yield return new WaitForSeconds(flickerInterval * Random.Range(0.5f, 1.5f));
        }
    }

    private void SaveLights()
    {
        originalIntensities = new float[lights.Length];
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null) originalIntensities[i] = lights[i].intensity;
        }
    }

    private void SetLights(float intensityMultiplier, Color? color)
    {
        for (int i = 0; i < lights.Length; i++)
        {
            Light2D target = lights[i];
            if (target == null) continue;

            target.intensity = originalIntensities[i] * intensityMultiplier;
            if (color.HasValue) target.color = color.Value;
        }
    }

    private void LockPlayer()
    {
        if (Player.Instance == null) return;

        lockedMovement = Player.Instance.Movement;
        lockedMovement.AddLock(this);
    }

    private void UnlockPlayer()
    {
        if (lockedMovement == null) return;

        lockedMovement.RemoveLock(this);
        lockedMovement = null;
    }
}
