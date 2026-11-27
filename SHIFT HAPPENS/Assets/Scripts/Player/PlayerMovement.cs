using UnityEngine;

public class PlayerMovement : MonoBehaviour, ILockable
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sneakSpeed = 2.5f;

    private Rigidbody2D rb;
    private PlayerInputReader input;

    private Vector2 movement;
    private Vector2 lastMovement = Vector2.down;

    private bool isSneaking;

    private readonly LockSet locks = new LockSet();

    public LockTarget LockTarget => LockTarget.Movement;
    public bool IsLocked => locks.IsLocked;
    public bool IsMoving => enabled && CurrentMovement != Vector2.zero;
    public bool IsSneaking => isSneaking;

    public Vector2 CurrentMovement => IsLocked ? Vector2.zero : movement;
    public Vector2 Facing => lastMovement;

    public void AddLock(object source)
    {
        locks.Add(source);
    }

    public void RemoveLock(object source)
    {
        locks.Remove(source);
    }

    public void Face(Vector2 direction)
    {
        if (direction == Vector2.zero) return;

        lastMovement = direction.normalized;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        input = GetComponent<PlayerInputReader>();
    }

    private void OnEnable()
    {
        Lockables.Register(this);
    }

    private void OnDisable()
    {
        Lockables.Unregister(this);
    }

    private void Update()
    {
        movement = input.Move.normalized;
        isSneaking = input.IsSneakHeld;

        if (CurrentMovement != Vector2.zero)
        {
            lastMovement = CurrentMovement;
        }
    }

    private void FixedUpdate()
    {
        float currentSpeed = isSneaking ? sneakSpeed : moveSpeed;

        rb.linearVelocity = CurrentMovement * currentSpeed;
    }
}
