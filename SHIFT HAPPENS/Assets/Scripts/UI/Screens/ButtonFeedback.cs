using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class ButtonFeedback : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [Header("Outline")]
    [SerializeField] private bool showOutline = true;
    [SerializeField] private Color outlineColor = new Color(0.85f, 0.85f, 1f, 1f);
    [SerializeField, Min(0f)] private float outlineSize = 2f;

    [Header("Pop")]
    [SerializeField, Min(1f)] private float popScale = 1.05f;
    [SerializeField, Min(1f)] private float selectedScale = 1.02f;
    [SerializeField, Min(0f)] private float popDuration = 0.06f;
    [SerializeField, Min(0f)] private float settleDuration = 0.08f;

    [Header("Target")]
    [SerializeField] private Transform scaleTarget;

    private Outline outline;
    private Coroutine scaleRoutine;

    private Transform Target => scaleTarget != null ? scaleTarget : transform;

    private void Awake()
    {
        if (scaleTarget == null && GetComponent<Selectable>() is Slider slider && slider.handleRect != null)
            scaleTarget = slider.handleRect;

        if (!showOutline) return;

        GameObject outlineHost = Target.gameObject;
        outline = outlineHost.GetComponent<Outline>();
        if (outline == null) outline = outlineHost.AddComponent<Outline>();

        outline.effectColor = outlineColor;
        outline.effectDistance = new Vector2(outlineSize, -outlineSize);
        outline.enabled = false;
    }

    private void OnDisable()
    {
        scaleRoutine = null;
        Target.localScale = Vector3.one;
        SetOutline(false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        SetOutline(true);
        Animate(Pop());
    }

    public void OnDeselect(BaseEventData eventData)
    {
        SetOutline(false);
        Animate(ScaleTo(1f, settleDuration));
    }

    private void SetOutline(bool visible)
    {
        if (outline != null) outline.enabled = visible;
    }

    private void Animate(IEnumerator animation)
    {
        if (!isActiveAndEnabled) return;

        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(animation);
    }

    private IEnumerator Pop()
    {
        yield return ScaleTo(popScale, popDuration);
        yield return ScaleTo(selectedScale, settleDuration);
    }

    private IEnumerator ScaleTo(float target, float duration)
    {
        float start = Target.localScale.x;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            Target.localScale = Vector3.one * Mathf.Lerp(start, target, progress);
            yield return null;
        }

        Target.localScale = Vector3.one * target;
    }
}
