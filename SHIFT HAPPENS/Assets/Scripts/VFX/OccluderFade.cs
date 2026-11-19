using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class OccluderFade : MonoBehaviour
{
    [Tooltip("Sprite to fade. Defaults to the first SpriteRenderer in the parents.")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField, Range(0f, 1f)] private float fadedAlpha = 0.45f;
    [SerializeField] private float fadeDuration = 0.15f;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponentInParent<SpriteRenderer>();

        if (!GetComponent<Collider2D>().isTrigger)
        {
            Debug.LogWarning($"{name}: the OccluderFade collider should be a trigger.", this);
        }
    }

    private void OnDisable()
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = null;
        SetAlpha(1f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) StartFade(fadedAlpha);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) StartFade(1f);
    }

    private void StartFade(float target)
    {
        if (spriteRenderer == null) return;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = null;

        if (!gameObject.activeInHierarchy || !enabled)
        {
            SetAlpha(1f);
            return;
        }

        fadeRoutine = StartCoroutine(FadeTo(target));
    }

    private IEnumerator FadeTo(float target)
    {
        float speed = fadeDuration > 0f ? Mathf.Abs(1f - fadedAlpha) / fadeDuration : float.MaxValue;
        float alpha = spriteRenderer.color.a;

        while (!Mathf.Approximately(alpha, target))
        {
            alpha = Mathf.MoveTowards(alpha, target, speed * Time.deltaTime);
            SetAlpha(alpha);
            yield return null;
        }

        fadeRoutine = null;
    }

    private void SetAlpha(float alpha)
    {
        if (spriteRenderer == null) return;
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}
