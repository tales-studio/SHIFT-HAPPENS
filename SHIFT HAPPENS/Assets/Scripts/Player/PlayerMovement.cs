using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Transform interactCollider;

    private float colliderPosMult = 10f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movement;
    private Vector2 lastMovement = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (movement != Vector2.zero)
        {
            lastMovement = movement;
        }

        animator.SetFloat("MoveX", lastMovement.x);
        animator.SetFloat("MoveY", lastMovement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);

        UpdateInteractColliderPos();
    }

    private void UpdateInteractColliderPos()
    {
        interactCollider.localPosition = new Vector2(lastMovement.x * colliderPosMult, lastMovement.y * colliderPosMult);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = movement * moveSpeed;
    }

    public void OnMove(InputValue value)
    {
        movement = value.Get<Vector2>().normalized;
    }
}