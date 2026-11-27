using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerMovement), typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int Shift = Animator.StringToHash("Shift");

    private PlayerMovement movement;
    private Animator animator;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
    }

    private void LateUpdate()
    {
        if (!movement.enabled) return;

        Vector2 facing = movement.Facing;
        animator.SetFloat(MoveX, facing.x);
        animator.SetFloat(MoveY, facing.y);
        animator.SetFloat(Speed, movement.CurrentMovement.sqrMagnitude);
        animator.SetBool(Shift, movement.IsSneaking);
    }
}
