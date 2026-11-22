using UnityEngine;

[DisallowMultipleComponent]
public class BouncingDots : MonoBehaviour
{
    [SerializeField] private RectTransform[] dots = new RectTransform[0];
    [SerializeField, Min(0f)] private float height = 6f;
    [SerializeField, Min(0f)] private float speed = 6f;
    [SerializeField, Min(0f)] private float stagger = 0.6f;

    private Vector2[] basePositions;
    private float time;

    private void Awake()
    {
        basePositions = new Vector2[dots.Length];
        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] != null) basePositions[i] = dots[i].anchoredPosition;
        }
    }

    private void OnEnable()
    {
        time = 0f;
    }

    private void OnDisable()
    {
        if (basePositions == null) return;

        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] != null) dots[i].anchoredPosition = basePositions[i];
        }
    }

    private void Update()
    {
        time += Time.deltaTime;

        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] == null) continue;

            float wave = Mathf.Max(0f, Mathf.Sin(time * speed - i * stagger));
            dots[i].anchoredPosition = basePositions[i] + Vector2.up * wave * height;
        }
    }
}
