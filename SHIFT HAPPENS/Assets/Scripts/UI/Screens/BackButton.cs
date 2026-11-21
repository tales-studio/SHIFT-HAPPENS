using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BackButton : MonoBehaviour
{
    private MenuNavigator navigator;

    private void Awake()
    {
        navigator = GetComponentInParent<MenuNavigator>();
        GetComponent<Button>().onClick.AddListener(Back);
    }

    private void Back()
    {
        if (navigator != null) navigator.Pop();
    }
}
