using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sneakSpeed = 2.5f;

    private Rigidbody2D rb;
    private Animator animator;

    private Vector2 movement;
    private Vector2 lastMovement = Vector2.down;

    private bool isSneaking;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        isSneaking = Keyboard.current.leftShiftKey.isPressed;

        if (movement != Vector2.zero)
        {
            lastMovement = movement;
        }

        animator.SetFloat("MoveX", lastMovement.x);
        animator.SetFloat("MoveY", lastMovement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);

        animator.SetBool("Shift", isSneaking);
    }

    private void FixedUpdate()
    {
        float currentSpeed = isSneaking ? sneakSpeed : moveSpeed;

        rb.linearVelocity = movement * currentSpeed;
    }

    public void OnMove(InputValue value)
    {
        movement = value.Get<Vector2>().normalized;
    }

    public void OnSneak(InputValue value)
    {
        Debug.Log("Sneak callback: " + value.isPressed);

        isSneaking = value.isPressed;

        Debug.Log("isSneaking actual: " + isSneaking);
    }
}