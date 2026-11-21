using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class ButtonFeedback : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [Header("Outline")]
    [SerializeField] private Color outlineColor = new Color(0.85f, 0.85f, 1f, 1f);
    [SerializeField, Min(0f)] private float outlineSize = 2f;

    [Header("Pop")]
    [SerializeField, Min(1f)] private float popScale = 1.05f;
    [SerializeField, Min(1f)] private float selectedScale = 1.02f;
    [SerializeField, Min(0f)] private float popDuration = 0.06f;
    [SerializeField, Min(0f)] private float settleDuration = 0.08f;

    private Outline outline;
    private Coroutine scaleRoutine;

    private void Awake()
    {
        outline = GetComponent<Outline>();
        if (outline == null) outline = gameObject.AddComponent<Outline>();

        outline.effectColor = outlineColor;
        outline.effectDistance = new Vector2(outlineSize, -outlineSize);
        outline.enabled = false;
    }

    private void OnDisable()
    {
        scaleRoutine = null;
        transform.localScale = Vector3.one;
        outline.enabled = false;
    }

    public void OnSelect(BaseEventData eventData)
    {
        outline.enabled = true;
        Animate(Pop());
    }

    public void OnDeselect(BaseEventData eventData)
    {
        outline.enabled = false;
        Animate(ScaleTo(1f, settleDuration));
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
        float start = transform.localScale.x;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            transform.localScale = Vector3.one * Mathf.Lerp(start, target, progress);
            yield return null;
        }

        transform.localScale = Vector3.one * target;
    }
}
