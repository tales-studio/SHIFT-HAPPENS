using UnityEngine;
using UnityEngine.InputSystem;

public interface IFlashlightTarget
{
    void OnLit();
}

// Gira la linterna hacia el ratón y avisa a lo que toca su haz.
// Funciona con cámara ortográfica y en perspectiva.
[RequireComponent(typeof(Collider2D))]
public class FlashlightLogic : MonoBehaviour
{
    [Tooltip("Opcional: si está vacío usa Camera.main (la del CinemachineBrain).")]
    [SerializeField] private Camera cam;

    [Tooltip("Opcional: si está vacío lo busca en el jugador.")]
    [SerializeField] private PlayerInput playerInput;

    [SerializeField] private string aimActionName = "Aim";

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
        // Si la linterna se instancia después, no tiene referencias de escena: las buscamos
        if (playerInput == null) playerInput = GetComponentInParent<PlayerInput>();
        if (playerInput == null) playerInput = FindFirstObjectByType<PlayerInput>();

        if (playerInput != null)
            aimAction = playerInput.actions.FindAction(aimActionName);
        else
            Debug.LogWarning(name + ": la linterna no encuentra el PlayerInput.", this);
    }

    private void Update()
    {
        if (aimAction == null || !aimAction.enabled) return;

        Camera camera = cam != null ? cam : Camera.main;
        if (camera == null) return;

        // Rayo desde la cámara a través del ratón
        Ray ray = camera.ScreenPointToRay(aimAction.ReadValue<Vector2>());

        // Plano del juego
        Plane gamePlane = new Plane(Vector3.forward, transform.position);
        if (!gamePlane.Raycast(ray, out float distance)) return;

        Vector2 mouseWorld = ray.GetPoint(distance);
        Vector2 direction = mouseWorld - (Vector2)transform.position;
        if (direction.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isOn) return;

        IFlashlightTarget target = other.GetComponentInParent<IFlashlightTarget>();
        if (target != null) target.OnLit();
    }
}