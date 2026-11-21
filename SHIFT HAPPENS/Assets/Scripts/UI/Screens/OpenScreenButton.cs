using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class OpenScreenButton : MonoBehaviour
{
    [SerializeField] private UIScreen target;

    private MenuNavigator navigator;

    private void Awake()
    {
        navigator = GetComponentInParent<MenuNavigator>();
        GetComponent<Button>().onClick.AddListener(Open);
    }

    private void Open()
    {
        if (navigator != null) navigator.Push(target);
    }
}
