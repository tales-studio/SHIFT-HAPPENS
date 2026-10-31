using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightMovement : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] private PlayerInput playerInput;
    private InputAction aimAction;

    private void Awake()
    {
        aimAction = playerInput.actions["Aim"];
    }

    private void Update()
    {
        Vector2 mouseScreen = aimAction.ReadValue<Vector2>();
        Debug.Log(mouseScreen);
        Vector2 mouseWorld = cam.ScreenToWorldPoint(mouseScreen);
        Vector2 direction = mouseWorld - (Vector2)this.transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        this.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }
}
