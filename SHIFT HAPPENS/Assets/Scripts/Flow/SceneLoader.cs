using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class SceneLoader : MonoBehaviour, ISceneLoader
{
    [Serializable]
    private struct SceneEntry
    {
        public SceneId id;
        public string sceneName;
    }

    [SerializeField] private SceneEntry[] scenes;

    public bool IsLoading { get; private set; }

    private void Awake()
    {
        SceneServices.Register(this);
    }

    private void OnDestroy()
    {
        SceneServices.Unregister(this);
    }

    public void Load(SceneId id)
    {
        if (!TryGetSceneName(id, out string sceneName))
        {
            Debug.LogError("SceneLoader: no scene assigned to " + id + ".", this);
            return;
        }

        StartLoad(sceneName);
    }

    public void Reload()
    {
        StartLoad(SceneManager.GetActiveScene().name);
    }

    private void StartLoad(string sceneName)
    {
        if (IsLoading) return;

        StartCoroutine(LoadRoutine(sceneName));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        IsLoading = true;
        IScreenFader fader = SceneServices.Fader;

        yield return fader.FadeOut();

        Time.timeScale = 1f;
        AudioServices.Music.StopMusic();

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        if (operation != null) yield return operation;

        yield return fader.FadeIn();

        IsLoading = false;
    }

    private bool TryGetSceneName(SceneId id, out string sceneName)
    {
        foreach (SceneEntry entry in scenes)
        {
            if (entry.id == id)
            {
                sceneName = entry.sceneName;
                return true;
            }
        }

        sceneName = null;
        return false;
    }
}
