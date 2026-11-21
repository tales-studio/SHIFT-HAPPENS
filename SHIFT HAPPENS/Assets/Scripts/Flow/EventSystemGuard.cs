using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(EventSystem))]
public class EventSystemGuard : MonoBehaviour
{
    private EventSystem own;

    private void Awake()
    {
        own = GetComponent<EventSystem>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Start()
    {
        RemoveDuplicates();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RemoveDuplicates();
    }

    private void RemoveDuplicates()
    {
        foreach (EventSystem other in FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
        {
            if (other != own) Destroy(other.gameObject);
        }
    }
}
