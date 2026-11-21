using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ReloadSceneButton : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Reload);
    }

    private void Reload()
    {
        SceneServices.Loader.Reload();
    }
}
