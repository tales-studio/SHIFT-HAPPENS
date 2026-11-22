using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class HudGroup : MonoBehaviour
{
    [SerializeField, Min(0f)] private float fadeDuration = 0.4f;
    [SerializeField] private bool startHidden;

    private CanvasGroup group;
    private Coroutine fadeRoutine;

    public bool IsVisible { get; private set; } = true;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
        SetVisible(!startHidden, true);
    }

    private void OnDisable()
    {
        if (fadeRoutine == null) return;

        fadeRoutine = null;
        ApplyAlpha(IsVisible ? 1f : 0f);
    }

    public void Show(bool instant = false) => SetVisible(true, instant);

    public void Hide(bool instant = false) => SetVisible(false, instant);

    public void SetVisible(bool visible, bool instant = false)
    {
        IsVisible = visible;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        float target = visible ? 1f : 0f;

        if (instant || fadeDuration <= 0f || !isActiveAndEnabled)
        {
            ApplyAlpha(target);
            return;
        }

        fadeRoutine = StartCoroutine(Fade(target));
    }

    private IEnumerator Fade(float target)
    {
        float start = group.alpha;
        float elapsed = 0f;

        group.interactable = false;
        group.blocksRaycasts = false;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / fadeDuration));
            group.alpha = Mathf.Lerp(start, target, progress);
            yield return null;
        }

        ApplyAlpha(target);
        fadeRoutine = null;
    }

    private void ApplyAlpha(float alpha)
    {
        group.alpha = alpha;
        bool visible = alpha > 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
    }
}
