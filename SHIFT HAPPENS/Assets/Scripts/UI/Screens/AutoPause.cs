using UnityEngine;

[DisallowMultipleComponent]
public class AutoPause : MonoBehaviour
{
    private bool wasFullScreen;

    private void Start()
    {
        wasFullScreen = Screen.fullScreen;
    }

    private void Update()
    {
        bool fullScreen = Screen.fullScreen;
        if (wasFullScreen && !fullScreen) PauseGame();
        wasFullScreen = fullScreen;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) PauseGame();
    }

    private static void PauseGame()
    {
        if (GameManager.Instance != null) GameManager.Instance.Pause();
    }
}
