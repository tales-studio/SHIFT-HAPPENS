using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SpriteFlipbook : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField, Min(0.1f)] private float framesPerSecond = 8f;

    private Image image;
    private int frameIndex;
    private float timer;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        frameIndex = 0;
        timer = 0f;
        ShowFrame();
    }

    private void Update()
    {
        if (frames == null || frames.Length < 2) return;

        timer += Time.unscaledDeltaTime;
        float frameDuration = 1f / framesPerSecond;

        while (timer >= frameDuration)
        {
            timer -= frameDuration;
            frameIndex = (frameIndex + 1) % frames.Length;
            ShowFrame();
        }
    }

    private void ShowFrame()
    {
        if (frames == null || frames.Length == 0) return;

        image.sprite = frames[frameIndex];
    }
}
