using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour, ILockable
{
    [SerializeField] private Collider2D interactionArea;
    [SerializeField] private float areaDistance = 2f;

    [Tooltip("Layers whose colliders count as interactable. Exclude helper triggers like FadeZone.")]
    [SerializeField] private LayerMask interactableLayers = ~0;

    private readonly List<Collider2D> overlaps = new List<Collider2D>();
    private readonly LockSet locks = new LockSet();
    private ContactFilter2D filter;
    private PlayerInputReader input;
    private PlayerMovement movement;
    private InteractSign sign;

    public LockTarget LockTarget => LockTarget.Interaction;
    public bool IsLocked => locks.IsLocked;

    public void AddLock(object source)
    {
        locks.Add(source);
    }

    public void RemoveLock(object source)
    {
        locks.Remove(source);
    }

    private void Awake()
    {
        filter = new ContactFilter2D().NoFilter();
        filter.SetLayerMask(interactableLayers);
        filter.useTriggers = true;

        input = GetComponent<PlayerInputReader>();
        movement = GetComponent<PlayerMovement>();
        sign = GetComponent<InteractSign>();
    }

    private void OnEnable()
    {
        input.Interacted += HandleInteract;
        Lockables.Register(this);
    }

    private void OnDisable()
    {
        input.Interacted -= HandleInteract;
        Lockables.Unregister(this);
        if (sign != null) sign.Hide();
    }

    private void LateUpdate()
    {
        PlaceArea();

        IInteractable closest = IsLocked ? null : GetClosestInteractable();
        if (sign == null) return;

        if (closest == null) sign.Hide();
        else sign.Show(closest.SignPosition);
    }

    private void PlaceArea()
    {
        if (movement == null) return;

        interactionArea.transform.localPosition = movement.Facing * areaDistance;
    }

    private void HandleInteract()
    {
        if (IsLocked) return;

        IInteractable closest = GetClosestInteractable();

        if (closest != null)
        {
            closest.Interact();
        }
    }

    private IInteractable GetClosestInteractable()
    {
        IInteractable closest = null;
        float minSqrDistance = float.MaxValue;
        Vector2 position = transform.position;

        interactionArea.Overlap(filter, overlaps);

        foreach (Collider2D overlap in overlaps)
        {
            IInteractable interactable = overlap.GetComponentInParent<IInteractable>();
            if (interactable == null || !interactable.CanInteract) continue;

            float sqrDistance = (position - (Vector2)interactable.GetTransform().position).sqrMagnitude;

            if (sqrDistance < minSqrDistance)
            {
                minSqrDistance = sqrDistance;
                closest = interactable;
            }
        }
        return closest;
    }
}
