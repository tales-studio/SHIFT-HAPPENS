using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class InputContextService : MonoBehaviour
{
    public static InputContextService Instance { get; private set; }

    [SerializeField] private InputActionAsset actions;
    [SerializeField] private InputActionReference pauseAction;
    [SerializeField] private InputActionReference backAction;
    [SerializeField] private InputActionReference skipAction;

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
                return Is(action, pauseAction) || Is(action, backAction);
            case InputContext.Cutscene:
                return Is(action, skipAction);
            default:
                return false;
        }
    }

    private static bool Is(InputAction action, InputActionReference reference)
    {
        return reference != null && reference.action != null && action.id == reference.action.id;
    }

    private void ApplyActions(InputContext context)
    {
        if (actions == null) return;

        foreach (InputActionMap map in actions.actionMaps)
        {
            foreach (InputAction action in map.actions)
            {
                if (IsAllowed(context, action)) action.Enable();
                else action.Disable();
            }
        }
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
