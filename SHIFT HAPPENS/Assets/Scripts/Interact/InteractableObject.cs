using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [SerializeField] private bool canInteract = true;

    public UnityEvent onInteract;

    private Vector3 signLocalAnchor;

    public bool CanInteract => canInteract && isActiveAndEnabled;

    public Vector3 SignPosition => transform.TransformPoint(signLocalAnchor);

    private void Awake()
    {
        signLocalAnchor = transform.InverseTransformPoint(MeasureSignAnchor());
    }

    private Vector3 MeasureSignAnchor()
    {
        Renderer[] visuals = GetComponentsInChildren<SpriteRenderer>();

        if (visuals.Length == 0)
        {
            SpriteRenderer parentVisual = GetComponentInParent<SpriteRenderer>();
            if (parentVisual == null) return transform.position;
            visuals = new Renderer[] { parentVisual };
        }

        Bounds bounds = visuals[0].bounds;
        for (int i = 1; i < visuals.Length; i++)
        {
            bounds.Encapsulate(visuals[i].bounds);
        }

        return new Vector3(bounds.center.x, bounds.max.y, transform.position.z);
    }

    public void SetCanInteract(bool value)
    {
        canInteract = value;
    }

    public void Interact()
    {
        if (!CanInteract) return;
        onInteract.Invoke();
    }

    public Transform GetTransform()
    {
        return transform;
    }
}
