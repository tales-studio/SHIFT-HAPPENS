using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class InputContextService : MonoBehaviour
{
    public static InputContextService Instance { get; private set; }

    [SerializeField] private InputActionAsset actions;
    [SerializeField] private InputActionReference[] menuActions;
    [SerializeField] private InputActionReference[] cutsceneActions;

    private GameManager gameManager;
    private ICutscenePlayer cutscenePlayer;
    private BaseInputModule blockedModule;
    private bool cutscenePlaying;

    public InputContext Current { get; private set; } = InputContext.Idle;

    public event Action<InputContext> Changed;

    private void Awake()
    {
        Instance = this;
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
        cutscenePlayer = CutsceneServices.Player;
        if (cutscenePlayer != null)
        {
            cutscenePlayer.Started += HandleCutsceneStarted;
            cutscenePlayer.Finished += HandleCutsceneFinished;
        }

        BindGameManager(GameManager.Instance);
    }

    private void OnDestroy()
    {
        if (cutscenePlayer != null)
        {
            cutscenePlayer.Started -= HandleCutsceneStarted;
            cutscenePlayer.Finished -= HandleCutsceneFinished;
        }

        BindGameManager(null);
        UnblockMenus();
        if (Instance == this) Instance = null;
    }

    public void Refresh()
    {
        InputContext context = Resolve();
        ApplyActions(context);
        ApplyMenus(context);

        if (context == Current) return;

        Current = context;
        Changed?.Invoke(context);
    }

    private InputContext Resolve()
    {
        if (cutscenePlaying) return InputContext.Cutscene;
        if (gameManager == null) return InputContext.Idle;
        if (gameManager.CurrentPhase == GamePhase.Ended) return InputContext.Ended;
        if (gameManager.IsPaused) return InputContext.Paused;
        if (gameManager.isPlaying) return InputContext.Gameplay;

        return InputContext.Idle;
    }

    private bool IsAllowed(InputContext context, InputAction action)
    {
        switch (context)
        {
            case InputContext.Gameplay:
                return true;
            case InputContext.Paused:
            case InputContext.Ended:
                return Contains(menuActions, action);
            case InputContext.Cutscene:
                return Contains(cutsceneActions, action);
            default:
                return false;
        }
    }

    private static bool Contains(InputActionReference[] references, InputAction action)
    {
        if (references == null) return false;

        foreach (InputActionReference reference in references)
            if (Matches(reference, action)) return true;

        return false;
    }

    private void ApplyActions(InputContext context)
    {
        if (actions == null) return;

        InputSystemUIInputModule uiModule = FindUIModule();

        foreach (InputActionMap map in actions.actionMaps)
        {
            foreach (InputAction action in map.actions)
            {
                if (IsUIAction(uiModule, action)) continue;

                if (IsAllowed(context, action)) action.Enable();
                else action.Disable();
            }
        }
    }

    private static InputSystemUIInputModule FindUIModule()
    {
        EventSystem eventSystem = EventSystem.current;
        return eventSystem != null ? eventSystem.GetComponent<InputSystemUIInputModule>() : null;
    }

    private static bool IsUIAction(InputSystemUIInputModule module, InputAction action)
    {
        if (module == null) return false;

        return Matches(module.point, action)
            || Matches(module.move, action)
            || Matches(module.submit, action)
            || Matches(module.cancel, action)
            || Matches(module.leftClick, action)
            || Matches(module.middleClick, action)
            || Matches(module.rightClick, action)
            || Matches(module.scrollWheel, action)
            || Matches(module.trackedDevicePosition, action)
            || Matches(module.trackedDeviceOrientation, action);
    }

    private static bool Matches(InputActionReference reference, InputAction action)
    {
        return reference != null && reference.action != null && reference.action.id == action.id;
    }

    private void ApplyMenus(InputContext context)
    {
        if (context == InputContext.Cutscene) BlockMenus();
        else UnblockMenus();
    }

    private void BlockMenus()
    {
        if (blockedModule != null) return;

        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) return;

        eventSystem.SetSelectedGameObject(null);

        BaseInputModule module = eventSystem.GetComponent<BaseInputModule>();
        if (module == null || !module.enabled) return;

        module.enabled = false;
        blockedModule = module;
    }

    private void UnblockMenus()
    {
        if (blockedModule != null) blockedModule.enabled = true;
        blockedModule = null;
    }

    private void BindGameManager(GameManager manager)
    {
        if (gameManager != null)
        {
            gameManager.OnPaused -= Refresh;
            gameManager.OnResumed -= Refresh;
            gameManager.OnPhaseChanged -= HandlePhaseChanged;
        }

        gameManager = manager;

        if (gameManager != null)
        {
            gameManager.OnPaused += Refresh;
            gameManager.OnResumed += Refresh;
            gameManager.OnPhaseChanged += HandlePhaseChanged;
        }

        Refresh();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindGameManager(GameManager.Instance);
    }

    private void HandlePhaseChanged(GamePhase phase)
    {
        Refresh();
    }

    private void HandleCutsceneStarted(Cutscene cutscene)
    {
        cutscenePlaying = true;
        Refresh();
    }

    private void HandleCutsceneFinished(Cutscene cutscene)
    {
        cutscenePlaying = false;
        Refresh();
    }
}
