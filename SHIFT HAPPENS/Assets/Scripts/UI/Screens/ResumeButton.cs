using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ResumeButton : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Resume);
    }

    private void Resume()
    {
        if (GameManager.Instance != null) GameManager.Instance.Resume();
    }
}
