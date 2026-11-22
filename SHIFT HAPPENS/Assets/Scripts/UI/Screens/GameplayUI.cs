using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class GameplayUI : MonoBehaviour
{
    [SerializeField] private MenuNavigator navigator;

    [Header("Screens")]
    [SerializeField] private UIScreen pauseScreen;
    [SerializeField] private UIScreen survivedScreen;
    [SerializeField] private UIScreen failedScreen;
    [SerializeField] private GameObject survivedBackdrop;

    [Header("Input")]
    [SerializeField] private InputAction pauseAction = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");
    [SerializeField] private InputAction backAction = new InputAction("Back", InputActionType.Button, "<Gamepad>/buttonEast");

    private GameManager gameManager;

    private void OnEnable()
    {
        pauseAction.performed += HandlePauseInput;
        backAction.performed += HandleBackInput;
        pauseAction.Enable();
        backAction.Enable();
    }

    private void OnDisable()
    {
        pauseAction.performed -= HandlePauseInput;
        backAction.performed -= HandleBackInput;
        pauseAction.Disable();
        backAction.Disable();
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager == null) return;

        gameManager.OnPaused += HandlePaused;
        gameManager.OnResumed += HandleResumed;
        gameManager.OnShiftEnded += HandleShiftEnded;
    }

    private void OnDestroy()
    {
        if (gameManager == null) return;

        gameManager.OnPaused -= HandlePaused;
        gameManager.OnResumed -= HandleResumed;
        gameManager.OnShiftEnded -= HandleShiftEnded;
    }

    private void HandlePauseInput(InputAction.CallbackContext context)
    {
        if (gameManager == null) return;

        if (gameManager.CurrentPhase == GamePhase.Ended) navigator.Pop();
        else if (!gameManager.IsPaused) gameManager.Pause();
        else GoBack();
    }

    private void HandleBackInput(InputAction.CallbackContext context)
    {
        if (gameManager == null) return;

        if (gameManager.CurrentPhase == GamePhase.Ended) navigator.Pop();
        else if (gameManager.IsPaused) GoBack();
    }

    private void GoBack()
    {
        if (!navigator.Pop()) gameManager.Resume();
    }

    private void HandlePaused()
    {
        navigator.CloseAll();
        navigator.Push(pauseScreen);
    }

    private void HandleResumed()
    {
        navigator.CloseAll();
    }

    private void HandleShiftEnded(ShiftResult result)
    {
        navigator.CloseAll();

        bool survived = result == ShiftResult.Survived;
        if (survivedBackdrop != null) survivedBackdrop.SetActive(survived);
        navigator.Push(survived ? survivedScreen : failedScreen);
    }
}
