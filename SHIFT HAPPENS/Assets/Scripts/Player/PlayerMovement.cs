using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sneakSpeed = 2.5f;

    [SerializeField] private Transform interactionArea;
    [SerializeField] private float colliderPosMult = 2f;

    private Rigidbody2D rb;
    private Animator animator;

    private Vector2 movement;
    private Vector2 lastMovement = Vector2.down;

    private bool isSneaking;

    private readonly HashSet<object> locks = new HashSet<object>();

    public bool IsLocked => locks.Count > 0;
    public bool IsMoving => enabled && CurrentMovement != Vector2.zero;
    public bool IsSneaking => isSneaking;

    private Vector2 CurrentMovement => IsLocked ? Vector2.zero : movement;

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
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (CurrentMovement != Vector2.zero)
        {
            lastMovement = CurrentMovement;
        }

        animator.SetFloat("MoveX", lastMovement.x);
        animator.SetFloat("MoveY", lastMovement.y);
        animator.SetFloat("Speed", CurrentMovement.sqrMagnitude);

        animator.SetBool("Shift", isSneaking);

        interactionArea.localPosition = new Vector2(
            lastMovement.x * colliderPosMult,
            lastMovement.y * colliderPosMult
        );
    }

    private void FixedUpdate()
    {
        float currentSpeed = isSneaking ? sneakSpeed : moveSpeed;

        rb.linearVelocity = CurrentMovement * currentSpeed;
    }

    public void OnMove(InputValue value)
    {
        movement = value.Get<Vector2>().normalized;
    }

    public void OnSneak(InputValue value)
    {
        isSneaking = value.isPressed;
    }
}
