using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class FloorBannerView : MonoBehaviour, ITransitionCard
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private FloorCatalog floorCatalog;
    [SerializeField, Min(0f)] private float fadeInDuration = 0.25f;
    [SerializeField, Min(0f)] private float holdDuration = 0.8f;
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.25f;
    [SerializeField, Min(0f)] private float skippableAfter = 0.3f;
    [SerializeField] private InputAction skipAction = new InputAction("Skip", InputActionType.Button);

    private CanvasGroup group;
    private float skipAllowedAt;
    private bool skipRequested;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
        SetAlpha(0f);

        if (skipAction.bindings.Count == 0)
        {
            skipAction.AddBinding("<Keyboard>/e");
            skipAction.AddBinding("<Gamepad>/buttonNorth");
        }
    }

    private void OnEnable()
    {
        SceneServices.Register(this);
        skipAction.performed += HandleSkip;
    }

    private void OnDisable()
    {
        SceneServices.Unregister(this);
        skipAction.performed -= HandleSkip;
        skipAction.Disable();
        SetAlpha(0f);
    }

    public IEnumerator Play(Vector2 from, Vector2 to)
    {
        LocationArea origin = LocationArea.At(from);
        LocationArea destination = LocationArea.At(to);

        if (origin == null || destination == null || origin.Floor == destination.Floor) yield break;

        if (titleText != null)
        {
            titleText.text = floorCatalog != null ? floorCatalog.GetName(destination.Floor) : destination.Floor.ToString();
        }

        skipRequested = false;
        skipAllowedAt = Time.unscaledTime + skippableAfter;
        skipAction.Enable();

        yield return FadeTo(1f, fadeInDuration);

        float held = 0f;
        while (held < holdDuration && !skipRequested)
        {
            held += Time.unscaledDeltaTime;
            yield return null;
        }

        skipAction.Disable();
        yield return FadeTo(0f, fadeOutDuration);
    }

    private void HandleSkip(InputAction.CallbackContext context)
    {
        if (Time.unscaledTime >= skipAllowedAt) skipRequested = true;
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
