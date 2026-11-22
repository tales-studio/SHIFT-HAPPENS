using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class StartGameButton : MonoBehaviour
{
    [SerializeField] private Cutscene intro;
    [SerializeField] private SceneId scene = SceneId.Gameplay;

    private readonly ICutsceneProgress progress = new PlayerPrefsCutsceneProgress();

    private bool starting;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(StartGame);
    }

    private void StartGame()
    {
        if (starting || SceneServices.Loader.IsLoading) return;

        starting = true;
        StartCoroutine(StartRoutine());
    }

    private IEnumerator StartRoutine()
    {
        if (intro != null && !progress.HasSeen(intro))
        {
            yield return SceneServices.Fader.FadeOut();
            AudioServices.Music.StopMusic();

            yield return CutsceneServices.Player.Play(intro);
            progress.MarkSeen(intro);
        }

        SceneServices.Loader.Load(scene);
    }
}
