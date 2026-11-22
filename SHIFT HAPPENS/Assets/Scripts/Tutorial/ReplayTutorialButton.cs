using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ReplayTutorialButton : MonoBehaviour
{
    [SerializeField] private SceneId scene = SceneId.Gameplay;

    private readonly ITutorialProgress progress = new PlayerPrefsTutorialProgress();
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(Replay);
    }

    private void OnEnable()
    {
        TutorialDirector director = FindFirstObjectByType<TutorialDirector>();
        button.interactable = director == null || director.CanReplay;
    }

    private void Replay()
    {
        TutorialDirector director = FindFirstObjectByType<TutorialDirector>();

        if (director != null)
        {
            director.Replay();
            return;
        }

        progress.Reset();
        SceneServices.Loader.Load(scene);
    }
}
