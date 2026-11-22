using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[DisallowMultipleComponent]
public class VideoCutscenePlayer : MonoBehaviour, ICutscenePlayer
{
    [SerializeField] private VideoPlayer video;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private GameObject screen;
    [SerializeField] private RawImage display;
    [SerializeField] private AspectRatioFitter fitter;

    private Cutscene current;
    private RenderTexture texture;
    private bool finished;

    public bool IsPlaying { get; private set; }

    public event Action<Cutscene> Started;
    public event Action<Cutscene> Finished;

    private void Awake()
    {
        video.playOnAwake = false;
        video.isLooping = false;
        video.renderMode = VideoRenderMode.RenderTexture;
        video.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;
        video.audioOutputMode = VideoAudioOutputMode.AudioSource;
        video.SetTargetAudioSource(0, audioSource);

        screen.SetActive(false);
        CutsceneServices.Register(this);
    }

    private void OnEnable()
    {
        video.loopPointReached += HandleVideoEnded;
        video.errorReceived += HandleVideoError;
    }

    private void OnDisable()
    {
        video.loopPointReached -= HandleVideoEnded;
        video.errorReceived -= HandleVideoError;
    }

    private void OnDestroy()
    {
        CutsceneServices.Unregister(this);
    }

    public IEnumerator Play(Cutscene cutscene)
    {
        if (IsPlaying || cutscene == null) yield break;

        if (!cutscene.HasVideo)
        {
            Debug.LogWarning("VideoCutscenePlayer: " + cutscene.name + " has no video file.", cutscene);
            yield break;
        }

        IsPlaying = true;
        StartCoroutine(PlayRoutine(cutscene));

        while (IsPlaying) yield return null;
    }

    public void Skip()
    {
        if (!IsPlaying || current == null || !current.Skippable) return;

        finished = true;
    }

    private IEnumerator PlayRoutine(Cutscene cutscene)
    {
        current = cutscene;
        finished = false;

        video.source = VideoSource.Url;
        video.url = cutscene.Url;
        video.Prepare();
        while (!video.isPrepared && !finished) yield return null;

        if (!finished)
        {
            CreateTexture((int)video.width, (int)video.height);
            screen.SetActive(true);
            video.Play();
            Started?.Invoke(cutscene);

            yield return SceneServices.Fader.FadeIn();

            while (!finished) yield return null;

            yield return SceneServices.Fader.FadeOut();
        }

        video.Stop();
        screen.SetActive(false);
        ReleaseTexture();

        current = null;
        IsPlaying = false;
        Finished?.Invoke(cutscene);
    }

    private void CreateTexture(int videoWidth, int videoHeight)
    {
        int width = Mathf.Max(1, videoWidth);
        int height = Mathf.Max(1, videoHeight);

        texture = new RenderTexture(width, height, 0);
        video.targetTexture = texture;
        display.texture = texture;
        fitter.aspectRatio = (float)width / height;
    }

    private void ReleaseTexture()
    {
        video.targetTexture = null;
        display.texture = null;

        if (texture == null) return;
        texture.Release();
        Destroy(texture);
        texture = null;
    }

    private void HandleVideoEnded(VideoPlayer source)
    {
        finished = true;
    }

    private void HandleVideoError(VideoPlayer source, string message)
    {
        Debug.LogError("VideoCutscenePlayer: " + message, this);
        finished = true;
    }
}
