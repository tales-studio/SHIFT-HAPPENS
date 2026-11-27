using UnityEngine;

public interface IFlashlightTarget
{
    void OnLit();
}

// Gira la linterna hacia el ratón (o el stick derecho) y avisa a lo que toca su haz.
// Funciona con cámara ortográfica y en perspectiva.
[RequireComponent(typeof(Collider2D))]
public class FlashlightLogic : MonoBehaviour
{
    [Tooltip("Opcional: si está vacío usa Camera.main (la del CinemachineBrain).")]
    [SerializeField] private Camera cam;

    [SerializeField, Range(0f, 1f)] private float stickDeadzone = 0.3f;

    [Tooltip("Desactívalo para simular la linterna apagada.")]
    [SerializeField] private bool isOn = true;

    private PlayerInputReader input;

    public bool IsOn
    {
        get => isOn;
        set => isOn = value;
    }

    private void Awake()
    {
        input = GetComponentInParent<PlayerInputReader>();
        if (input == null) Debug.LogWarning(name + ": la linterna no encuentra el PlayerInputReader.", this);
    }

    private void Update()
    {
        if (input == null || !input.IsAimActive) return;

        Vector2 direction;
        if (input.UsingGamepad)
        {
            direction = input.AimStick;
            if (direction.sqrMagnitude < stickDeadzone * stickDeadzone) return;
        }
        else if (!TryGetPointerDirection(out direction))
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private bool TryGetPointerDirection(out Vector2 direction)
    {
        direction = Vector2.zero;

        Camera camera = cam != null ? cam : Camera.main;
        if (camera == null) return false;

        // Rayo desde la cámara a través del ratón
        Ray ray = camera.ScreenPointToRay(input.AimScreenPosition);

        // Plano del juego
        Plane gamePlane = new Plane(Vector3.forward, transform.position);
        if (!gamePlane.Raycast(ray, out float distance)) return false;

        Vector2 mouseWorld = ray.GetPoint(distance);
        direction = mouseWorld - (Vector2)transform.position;
        return direction.sqrMagnitude >= 0.0001f;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isOn) return;

        IFlashlightTarget target = other.GetComponentInParent<IFlashlightTarget>();
        if (target != null) target.OnLit();
    }
}
