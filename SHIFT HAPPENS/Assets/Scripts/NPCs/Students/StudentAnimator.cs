using UnityEngine;
using UnityEngine.AI;

// Sirve para estudiantes y entidades.
[RequireComponent(typeof(NavMeshAgent))]
public class StudentAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Tooltip("Por debajo de esta velocidad se considera parado (evita temblores al llegar).")]
    [SerializeField] private float idleThreshold = 0.05f;

    [Tooltip("Suavizado del cambio de dirección. 0 = instantáneo.")]
    [SerializeField, Min(0f)] private float directionSmoothing = 10f;

    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int Speed = Animator.StringToHash("Speed");

    private NavMeshAgent agent;
    private Vector2 lastDirection = Vector2.down;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }


    private void Update()
    {
        if (animator == null || !agent.enabled || !agent.isOnNavMesh) return;


        // NavMeshAgent can retain residual velocity after its path is cleared.
        // With no active path the NPC should return to Idle immediately.
        bool hasMovementIntent = !agent.isStopped &&
                         (agent.hasPath || agent.pathPending);

        Vector2 velocity = hasMovementIntent
            ? new Vector2(agent.velocity.x, agent.velocity.y)
            : Vector2.zero;
        float speed = velocity.magnitude;

        // Solo actualizamos la dirección si se mueve: al pararse mantiene
        // la última, para que el idle mire hacia donde iba
        if (speed > idleThreshold)
        {
            Vector2 direction = velocity / speed;
            lastDirection = directionSmoothing > 0f
                ? Vector2.Lerp(lastDirection, direction, directionSmoothing * Time.deltaTime).normalized
                : direction;
        }

        animator.SetFloat(MoveX, lastDirection.x);
        animator.SetFloat(MoveY, lastDirection.y);

        // El jugador usa sqrMagnitude de un vector normalizado (0 o 1).
        // Aquí usamos lo rápido que va respecto a su velocidad máxima (0-1)
        float normalizedSpeed = agent.speed > 0f ? speed / agent.speed : 0f;
        animator.SetFloat(Speed, speed > idleThreshold ? normalizedSpeed : 0f);
    }
}