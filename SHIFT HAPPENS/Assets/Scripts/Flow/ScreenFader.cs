using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-200)]
[DisallowMultipleComponent]
public class ScreenFader : MonoBehaviour, IScreenFader
{
    private static ScreenFader instance;

    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField, Min(0f)] private float defaultDuration = 0.4f;

    private int fadeVersion;

    public bool IsFaded => fadePanel.alpha >= 1f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        SetAlpha(0f);
        SceneServices.Register(this);
    }

    private void OnDestroy()
    {
        if (instance != this) return;
        SceneServices.Unregister(this);
        instance = null;
    }

    public IEnumerator FadeOut(float duration = -1f) => FadeTo(1f, duration);

    public IEnumerator FadeIn(float duration = -1f) => FadeTo(0f, duration);

    private IEnumerator FadeTo(float target, float duration)
    {
        int version = ++fadeVersion;
        float time = duration < 0f ? defaultDuration : duration;
        float start = fadePanel.alpha;
        float elapsed = 0f;

        fadePanel.blocksRaycasts = true;

        while (elapsed < time)
        {
            if (version != fadeVersion) yield break;

            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / time));
            fadePanel.alpha = Mathf.Lerp(start, target, progress);
            yield return null;
        }

        if (version == fadeVersion) SetAlpha(target);
    }

    private void SetAlpha(float alpha)
    {
        fadePanel.alpha = alpha;
        fadePanel.blocksRaycasts = alpha > 0f;
        fadePanel.interactable = false;
    }
}
