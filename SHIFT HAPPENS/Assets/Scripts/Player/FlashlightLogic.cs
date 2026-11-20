using UnityEngine;
using UnityEngine.InputSystem;

public interface IFlashlightTarget
{
    void OnLit();
}

// Gira la linterna hacia el ratón y avisa a lo que toca su haz.
[RequireComponent(typeof(Collider2D))]
public class FlashlightLogic : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] private PlayerInput playerInput;

    [Tooltip("Desactívalo para simular la linterna apagada.")]
    [SerializeField] private bool isOn = true;

    private InputAction aimAction;

    public bool IsOn
    {
        get => isOn;
        set => isOn = value;
    }

    private void Awake()
    {
        aimAction = playerInput.actions["Aim"];
    }

    private void Update()
    {
        Vector2 mouseScreen = aimAction.ReadValue<Vector2>();
        Vector2 mouseWorld = cam.ScreenToWorldPoint(mouseScreen);
        Vector2 direction = mouseWorld - (Vector2)this.transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        this.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isOn) return;

        IFlashlightTarget target = other.GetComponentInParent<IFlashlightTarget>();
        if (target != null) target.OnLit();
    }
}