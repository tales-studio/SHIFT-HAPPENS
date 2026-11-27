using System.Collections;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class FloorBannerView : MonoBehaviour, ITransitionCard
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private FloorCatalog floorCatalog;
    [SerializeField, Min(0f)] private float fadeInDuration = 0.2f;
    [SerializeField, Min(0f)] private float holdDuration = 0.5f;
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.4f;

    private CanvasGroup group;
    private Coroutine playRoutine;
    private bool readyToReveal;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
        SetAlpha(0f);
    }

    private void OnEnable()
    {
        SceneServices.Register(this);
    }

    private void OnDisable()
    {
        SceneServices.Unregister(this);
        playRoutine = null;
        SetAlpha(0f);
    }

    public IEnumerator Play(Vector2 from, Vector2 to)
    {
        if (!isActiveAndEnabled) yield break;

        LocationArea origin = LocationArea.At(from);
        LocationArea destination = LocationArea.At(to);

        if (origin == null || destination == null || origin.Floor == destination.Floor) yield break;

        if (playRoutine != null) StopCoroutine(playRoutine);
        readyToReveal = false;
        playRoutine = StartCoroutine(PlayRoutine(destination.Floor));

        while (!readyToReveal && playRoutine != null)
        {
            yield return null;
        }
    }

    private IEnumerator PlayRoutine(BuildingFloor floor)
    {
        if (titleText != null)
        {
            titleText.text = floorCatalog != null ? floorCatalog.GetName(floor) : floor.ToString();
        }

        yield return FadeTo(1f, fadeInDuration);

        float held = 0f;
        while (held < holdDuration)
        {
            held += Time.unscaledDeltaTime;
            yield return null;
        }

        readyToReveal = true;
        yield return FadeTo(0f, fadeOutDuration);
        playRoutine = null;
    }

    private IEnumerator FadeTo(float target, float duration)
    {
        float start = group.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            group.alpha = Mathf.Lerp(start, target, progress);
            yield return null;
        }

        SetAlpha(target);
    }

    private void SetAlpha(float alpha)
    {
        group.alpha = alpha;
        group.interactable = false;
        group.blocksRaycasts = false;
    }
}
