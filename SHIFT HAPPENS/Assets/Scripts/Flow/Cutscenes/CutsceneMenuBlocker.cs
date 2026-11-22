using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class CutsceneMenuBlocker : MonoBehaviour
{
    private ICutscenePlayer player;
    private BaseInputModule blockedModule;

    private void Start()
    {
        player = CutsceneServices.Player;
        player.Started += HandleStarted;
        player.Finished += HandleFinished;
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.Started -= HandleStarted;
            player.Finished -= HandleFinished;
        }

        Unblock();
    }

    private void HandleStarted(Cutscene cutscene)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) return;

        eventSystem.SetSelectedGameObject(null);

        BaseInputModule module = eventSystem.GetComponent<BaseInputModule>();
        if (module == null || !module.enabled) return;

        module.enabled = false;
        blockedModule = module;
    }

    private void HandleFinished(Cutscene cutscene)
    {
        Unblock();
    }

    private void Unblock()
    {
        if (blockedModule != null) blockedModule.enabled = true;
        blockedModule = null;
    }
}
