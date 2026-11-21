using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

[DisallowMultipleComponent]
public class MenuNavigator : MonoBehaviour
{
    [SerializeField] private UIScreen firstScreen;
    [SerializeField] private bool backOnCancel = true;

    private readonly Stack<UIScreen> stack = new Stack<UIScreen>();
    private InputSystemUIInputModule module;
    private bool usingPointer;

    public UIScreen Current => stack.Count > 0 ? stack.Peek() : null;
    public int Depth => stack.Count;

    private void Start()
    {
        usingPointer = Mouse.current != null;

        foreach (UIScreen screen in GetComponentsInChildren<UIScreen>(true))
            screen.Hide();

        if (firstScreen != null) Push(firstScreen);

        SubscribeToInput();
    }

    private void OnDestroy()
    {
        if (module == null) return;

        Unsubscribe(module.point, HandlePoint);
        Unsubscribe(module.move, HandleMove);
        Unsubscribe(module.cancel, HandleCancel);
    }

    public void Push(UIScreen screen)
    {
        if (screen == null || screen == Current) return;

        if (Current != null) Current.Hide();
        stack.Push(screen);
        screen.Show(!usingPointer);
    }

    public bool Pop()
    {
        if (stack.Count <= 1) return false;

        stack.Pop().Hide();
        Current.Show(!usingPointer);
        return true;
    }

    public void CloseAll()
    {
        if (Current != null) Current.Hide();

        foreach (UIScreen screen in stack)
            screen.ForgetSelection();

        stack.Clear();
    }

    private void SubscribeToInput()
    {
        if (EventSystem.current == null) return;

        module = EventSystem.current.GetComponent<InputSystemUIInputModule>();
        if (module == null) return;

        Subscribe(module.point, HandlePoint);
        Subscribe(module.move, HandleMove);
        if (backOnCancel) Subscribe(module.cancel, HandleCancel);
    }

    private void HandlePoint(InputAction.CallbackContext context)
    {
        usingPointer = true;
    }

    private void HandleMove(InputAction.CallbackContext context)
    {
        usingPointer = false;

        if (EventSystem.current.currentSelectedGameObject == null && Current != null)
            Current.SelectDefault();
    }

    private void HandleCancel(InputAction.CallbackContext context)
    {
        usingPointer = false;
        Pop();
    }

    private static void Subscribe(InputActionReference reference, System.Action<InputAction.CallbackContext> handler)
    {
        if (reference != null && reference.action != null) reference.action.performed += handler;
    }

    private static void Unsubscribe(InputActionReference reference, System.Action<InputAction.CallbackContext> handler)
    {
        if (reference != null && reference.action != null) reference.action.performed -= handler;
    }
}
