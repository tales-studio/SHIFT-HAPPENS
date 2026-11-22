using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ReplayTutorialButton : MonoBehaviour
{
    [SerializeField] private SceneId scene = SceneId.Gameplay;

    private readonly ITutorialProgress progress = new PlayerPrefsTutorialProgress();

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Replay);
    }

    private void Replay()
    {
        progress.Reset();
        SceneServices.Loader.Load(scene);
    }
}
