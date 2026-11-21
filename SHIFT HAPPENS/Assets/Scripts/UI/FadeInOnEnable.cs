using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class FadeInOnEnable : MonoBehaviour
{
    [SerializeField, Min(0f)] private float duration = 0.25f;

    private Graphic graphic;
    private float targetAlpha;

    private void Awake()
    {
        graphic = GetComponent<Graphic>();
        targetAlpha = graphic.color.a;
    }

    private void OnEnable()
    {
        StartCoroutine(Fade());
    }

    private void OnDisable()
    {
        SetAlpha(targetAlpha);
    }

    private IEnumerator Fade()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            SetAlpha(Mathf.Lerp(0f, targetAlpha, elapsed / duration));
            yield return null;
            elapsed += Time.unscaledDeltaTime;
        }

        SetAlpha(targetAlpha);
    }

    private void SetAlpha(float alpha)
    {
        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }
}
