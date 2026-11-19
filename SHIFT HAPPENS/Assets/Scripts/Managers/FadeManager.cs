
using UnityEngine;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    [Header("Fade Configuration")]
    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private float fadeDuration = 0.4f;

    [Header("Player")]
    [SerializeField] private Behaviour playerMovement;

    private bool isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        fadePanel.alpha = 0f;
        fadePanel.blocksRaycasts = false;
        fadePanel.interactable = false;
    }

    public void Transition(
        Rigidbody2D player,
        Transform destination)
    {
        if (isTransitioning || player == null || destination == null)
            return;

        StartCoroutine(TransitionCoroutine(player, destination));
    }

    private IEnumerator TransitionCoroutine(
        Rigidbody2D player,
        Transform destination)
    {
        isTransitioning = true;

        bool movementWasEnabled =
            playerMovement != null && playerMovement.enabled;

        if (movementWasEnabled)
            playerMovement.enabled = false;

        fadePanel.blocksRaycasts = true;

        yield return Fade(0f, 1f);

        player.position = destination.position;

        yield return Fade(1f, 0f);

        fadePanel.blocksRaycasts = false;

        if (movementWasEnabled)
            playerMovement.enabled = true;

        isTransitioning = false;
    }

    private IEnumerator Fade(float start, float end)
    {
        float elapsed = 0f;

        if (fadeDuration <= 0f)
        {
            fadePanel.alpha = end;
            yield break;
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsed / fadeDuration
            );

            fadePanel.alpha = Mathf.Lerp(
                start,
                end,
                Mathf.SmoothStep(0f, 1f, progress)
            );

            yield return null;
        }

        fadePanel.alpha = end;
    }
}
