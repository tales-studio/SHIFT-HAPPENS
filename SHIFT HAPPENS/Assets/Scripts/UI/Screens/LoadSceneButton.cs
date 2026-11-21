using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LoadSceneButton : MonoBehaviour
{
    [SerializeField] private SceneId scene = SceneId.Gameplay;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Load);
    }

    private void Load()
    {
        SceneServices.Loader.Load(scene);
    }
}
