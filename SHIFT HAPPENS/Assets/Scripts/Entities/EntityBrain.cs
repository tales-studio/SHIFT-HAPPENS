using System;
using BehaviourAPI.Core;
using UnityEngine;
using UnityEngine.AI;

// Funciones propias comunes a todas las entidades.
[RequireComponent(typeof(NavMeshAgent))]
public abstract class EntityBrain : MonoBehaviour
{
    // Para el sistema de jumpscare
    public static event Action<EntityBrain> OnPlayerCaught;

    // La FSM ha terminado (bool = si llegó a atrapar a Ramón). Lo escucha el controller.
    public event Action<EntityBrain, bool> Finished;

    // Petición de interrupción. EntityBehaviour la convierte en push de la FSM.
    public event Action InterruptRequested;

    [Header("Opcional")]
    [Tooltip("Triggers opcionales: Appear, Attack, Disappear.")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected bool debugLogs = false;

    // Estructuras de datos de la especificación
    public EntityConfig Config { get; private set; }
    public StudentBrain AssociatedStudent { get; private set; }
    public Player TargetPlayer { get; private set; }
    public bool IsChasing { get; private set; }

    protected NavMeshAgent agent;
    private float timer;
    private float repathTimer;
    private bool caughtPlayer;
    private bool finishNotified;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false; // 2D (NavMeshPlus)
        agent.updateUpAxis = false;
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    // Lo llama el HallucinationController justo después de instanciarla
    public virtual void Begin(EntityConfig config, StudentBrain student, Player player)
    {
        Config = config;
        AssociatedStudent = student;
        TargetPlayer = player;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1f, agent.areaMask))
            agent.Warp(hit.position);

        Log("Aparece la entidad de " + (student != null ? student.name : "?"));
    }


    // Aparecer
    public virtual void StartAppear()
    {
        timer = 0f;
        StopMovement();
        SetTrigger("Appear");
    }

    public Status UpdateAppear() => Wait(Config.appearDuration);


    // Acechar (opcional): Solo lo usa el sonámbulo.
    public virtual bool UsesWatchState => false;
    public virtual void StartWatch() { }
    public virtual Status UpdateWatch() => Status.Success;
    public virtual void StopWatch() { }
    public virtual bool ShouldStartChase() => true;


    // Perseguir
    public virtual void StartChase()
    {
        IsChasing = true;
        repathTimer = 0f;
        agent.speed = Config.chaseSpeed;
        if (agent.isOnNavMesh) agent.isStopped = false;
    }

    public virtual Status UpdateChase()
    {
        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f && TargetPlayer != null && agent.isOnNavMesh)
        {
            repathTimer = Config.repathInterval;
            agent.SetDestination(TargetPlayer.Position);
        }
        return Status.Running; // Sale por transición (rango de ataque o interrupción)
    }

    public virtual void StopChase()
    {
        IsChasing = false;
        StopMovement();
    }


    // Percepciones pull
    public float DistanceToPlayer()
    {
        if (TargetPlayer == null) return float.MaxValue;
        return Vector2.Distance(transform.position, TargetPlayer.Position);
    }

    public bool PlayerInAttackRange()
    {
        return DistanceToPlayer() <= Config.attackRange;
    }


    // Atacar
    public virtual void StartAttack()
    {
        timer = 0f;
        caughtPlayer = true;
        StopMovement();
        SetTrigger("Attack");
        Log("Ataca a Ramón");
        OnPlayerCaught?.Invoke(this);
    }

    public Status UpdateAttack() => Wait(Config.attackDuration);


    // Desaparecer
    public virtual void StartDisappear()
    {
        timer = 0f;
        IsChasing = false;
        StopMovement();
        SetTrigger("Disappear");
        Log("Desaparece");
    }

    public Status UpdateDisappear() => Wait(Config.disappearDuration);


    // Comunicación

    // Percepciones que interrumpen un estado
    public void Interrupt()
    {
        InterruptRequested?.Invoke();
    }

    // Lo llama EntityBehaviour cuando la FSM sale por su transición de salida
    public void NotifyFinished()
    {
        if (finishNotified) return;
        finishNotified = true;
        Finished?.Invoke(this, caughtPlayer);
    }

    // Si la destruyen sin pasar por la FSM (recarga de escena, Destroy externo),
    // avisa igualmente para que el controller restaure al estudiante.
    protected virtual void OnDestroy()
    {
        NotifyFinished();
    }


    // Utilidades
    protected Status Wait(float duration)
    {
        timer += Time.deltaTime;
        return timer >= duration ? Status.Success : Status.Running;
    }

    protected void StopMovement()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    protected void SetTrigger(string trigger)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        foreach (AnimatorControllerParameter p in animator.parameters)
        {
            if (p.name == trigger && p.type == AnimatorControllerParameterType.Trigger)
            {
                animator.SetTrigger(p.nameHash);
                return;
            }
        }
    }

    protected void Log(string message)
    {
        if (debugLogs) Debug.Log("[" + name + "] " + message, this);
    }
}
