
using System;
using System.Collections.Generic;
using BehaviourAPI.Core;
using UnityEngine;

public enum StudentGender
{
    Unspecified,
    Male,
    Female
}

// Funciones propias del estudiante. StudentBehaviour monta el Utility System
// y el BT con la librería, pero todo lo que hacen es llamar a estos métodos.
//
//  - Utility System → GetTimeSinceLast() y ApplyModifiers()
//  - UtilityAction  → SetActivity()
//  - BT compartido  → RequestDestination(), UpdateMovement(), ... (leen CurrentActivity)
//  - Al terminar    → EndActivity()
[RequireComponent(typeof(NPCFloorNavigator))]
public class StudentBrain : MonoBehaviour
{
    // Constantes de diseño (antes eran configurables; de momento no hace falta)
    private const float RepeatPenalty = 0.2f;   // Si es la misma que la última
    private const float FailCooldown = 5f;      // Si no encontró destino o falló el camino

    [Header("Configuración")]
    [SerializeField] private StudentProfile profile;

    [Tooltip("Variante visual del estudiante; el comportamiento se comparte por arquetipo.")]
    [SerializeField] private StudentGender gender = StudentGender.Unspecified;

    [Tooltip("Habitación propia. Las actividades con destino Bedroom van aquí.")]
    [SerializeField] private NPCDestination ownBedroom;

    [Header("Actividades comunes (asignar una vez en el prefab base)")]
    [SerializeField] private ActivityData bathroom;
    [SerializeField] private ActivityData kitchen;
    [SerializeField] private ActivityData wander;

    [Header("Opcional")]
    [SerializeField] private Animator animator;
    [SerializeField] private bool debugLogs = false;


    // ------------------------------------------------------------------
    // Estado público
    // ------------------------------------------------------------------
    public StudentProfile Profile => profile;
    public StudentGender Gender => gender;
    public IReadOnlyList<ActivityEntry> Activities => activities;

    public ActivityData CurrentActivity { get; private set; }   // null = decidiendo
    public ActivityData LastActivity { get; private set; }      // historial: solo la última
    public bool IsOutbreak { get; private set; }
    public bool IsWaitingForRamon => waitingTask != null && waitingTask.isActive;

    // Para el futuro HallucinationController
    public bool BlocksEntity => CurrentActivity != null && CurrentActivity.blocksEntity;
    public event Action<StudentBrain, ActivityData> OnActivityFinished;

    // ------------------------------------------------------------------
    // Estado interno
    // ------------------------------------------------------------------
    private readonly List<ActivityEntry> activities = new List<ActivityEntry>();
    private readonly Dictionary<ActivityData, float> lastFinishTime = new Dictionary<ActivityData, float>();
    private readonly Dictionary<ActivityData, float> blockedUntil = new Dictionary<ActivityData, float>();
    private readonly Dictionary<ActivityData, float> noise = new Dictionary<ActivityData, float>();

    private NPCFloorNavigator navigator;
    private TaskLogic[] ownTasks;   // Tareas propias (hijos): apuntes, consolar...

    // Actividad en curso
    private NPCDestination destination;
    private bool arrived;
    private bool moveFailed;
    private float duration;
    private float timer;
    private float partEnd;
    private TaskLogic waitingTask;

    private float idleTimer;

    // ==================================================================
    // Unity
    // ==================================================================
    private void Awake()
    {
        navigator = GetComponent<NPCFloorNavigator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        ownTasks = GetComponentsInChildren<TaskLogic>(true);

        BuildActivityList();
        RollNoise();
    }

    private void OnEnable()
    {
        navigator.OnDestinationReached += HandleArrived;
        navigator.OnMovementFailed += HandleMoveFailed;
        navigator.OnMovementCancelled += HandleMoveFailed;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged += HandlePhaseChanged;
            IsOutbreak = GameManager.Instance.CurrentPhase == GamePhase.Outbreak;
        }
    }

    private void OnDisable()
    {
        navigator.OnDestinationReached -= HandleArrived;
        navigator.OnMovementFailed -= HandleMoveFailed;
        navigator.OnMovementCancelled -= HandleMoveFailed;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
    }

    // Comunes (con la preferencia del perfil) + exclusivas del perfil
    private void BuildActivityList()
    {
        activities.Clear();
        if (profile == null)
        {
            Debug.LogError(name + ": StudentBrain sin StudentProfile.", this);
            return;
        }

        AddActivity(bathroom, profile.bathroomPreference);
        AddActivity(kitchen, profile.kitchenPreference);
        AddActivity(wander, profile.wanderPreference);

        foreach (ActivityEntry entry in profile.exclusiveActivities)
            if (entry != null) AddActivity(entry.activity, entry.preference);
    }

    private void AddActivity(ActivityData data, float preference)
    {
        if (data == null) return;
        foreach (ActivityEntry e in activities)
            if (e.activity == data) return; // sin duplicados

        activities.Add(new ActivityEntry { activity = data, preference = preference });

        // Arrancan con algo de necesidad aleatoria para que no vayan todos a la vez
        float offset = data.timeToFullNeed > 0f ? UnityEngine.Random.Range(0f, data.timeToFullNeed * 0.7f) : 0f;
        lastFinishTime[data] = Time.time - offset;
    }

    private void HandlePhaseChanged(GamePhase phase)
    {
        IsOutbreak = phase == GamePhase.Outbreak;
    }

    // ==================================================================
    // UTILITY SYSTEM (percepciones pull)
    // ==================================================================

    // Segundos desde la última vez que la hizo (acotado a timeToFullNeed)
    public float GetTimeSinceLast(ActivityData data)
    {
        if (data.timeToFullNeed <= 0f) return 0f;
        float last = lastFinishTime.TryGetValue(data, out float t) ? t : Time.time;
        return Mathf.Clamp(Time.time - last, 0f, data.timeToFullNeed);
    }

    // Último eslabón de la cadena de factores de cada actividad
    public float ApplyModifiers(ActivityData data, float utility)
    {
        // Mientras hay actividad, no se cambia de idea: la actual 1 y el resto 0
        if (CurrentActivity != null)
            return data == CurrentActivity ? 1f : 0f;

        if (blockedUntil.TryGetValue(data, out float until) && Time.time < until)
            return 0f;

        if (!HasFreeDestination(data))
            return 0f;

        if (data == LastActivity && !data.allowRepeatInARow)
            utility *= RepeatPenalty;

        if (noise.TryGetValue(data, out float n))
            utility += n;

        return Mathf.Clamp01(utility);
    }

    // Percepción "Destino disponible". Un destino con su tarea activa
    // no está disponible (lo comprueba NPCDestination).
    private bool HasFreeDestination(ActivityData data)
    {
        if (data.destinationType == NPCDestinationType.Bedroom && ownBedroom != null)
            return ownBedroom.CanReserve(gameObject);

        if (NPCDestinationManager.Instance == null) return false;

        foreach (BuildingFloor floor in GetFloorsToSearch())
            if (NPCDestinationManager.Instance.HasAvailableDestination(data.destinationType, floor, gameObject))
                return true;

        return false;
    }

    private void RollNoise()
    {
        noise.Clear();
        float max = profile != null ? profile.decisionNoise : 0f;
        foreach (ActivityEntry e in activities)
            noise[e.activity] = UnityEngine.Random.Range(-max, max);
    }

    // ==================================================================
    // UTILITY ACTION: fija la actividad antes de arrancar el BT compartido
    // ==================================================================
    public void SetActivity(ActivityData data)
    {
        CurrentActivity = data;
        destination = null;
        arrived = false;
        moveFailed = false;
        waitingTask = null;
        timer = 0f;
        duration = data.RandomDuration(profile != null ? profile.durationMultiplier : 1f);

        Log("Empieza " + data.name + " (" + duration.ToString("0.0") + " s)");
    }

    // ==================================================================
    // BT COMPARTIDO: acciones (todas leen CurrentActivity)
    // ==================================================================

    // 1. Solicitar destino (+ inicia el movimiento)
    public Status RequestDestination()
    {
        ActivityData data = CurrentActivity;

        if (data.destinationType == NPCDestinationType.Bedroom && ownBedroom != null)
        {
            if (navigator.MoveToDestination(ownBedroom)) destination = ownBedroom;
        }
        else if (NPCDestinationManager.Instance != null)
        {
            foreach (BuildingFloor floor in GetFloorsToSearch())
            {
                if (!NPCDestinationManager.Instance.HasAvailableDestination(data.destinationType, floor, gameObject))
                    continue;

                if (NPCDestinationManager.Instance.TryRequestRandomDestination(
                        data.destinationType, floor, navigator, out NPCDestination found))
                {
                    destination = found;
                    break;
                }
            }
        }

        return destination != null ? Status.Success : Status.Failure;
    }

    // 2. Desplazarse (espera la percepción push de llegada)
    public Status UpdateMovement()
    {
        if (moveFailed) return Status.Failure;
        return arrived ? Status.Success : Status.Running;
    }

    // 3. Realizar actividad (1ª parte): hasta el momento de pedir ayuda
    public void StartFirstPart()
    {
        SetAnimation(true);
        switch (CurrentActivity.interactionMode)
        {
            case ActivityInteractionMode.MidActivity: partEnd = duration * CurrentActivity.interactionAt; break;
            case ActivityInteractionMode.Blocking:    partEnd = 0f; break;
            default:                                  partEnd = duration; break;
        }
    }

    // 5. Realizar actividad (2ª parte): lo que queda
    public void StartSecondPart()
    {
        // Bloqueante (llorar): al recibir ayuda ya ha terminado
        if (CurrentActivity.interactionMode == ActivityInteractionMode.Blocking && waitingTask != null)
            partEnd = timer;
        else
            partEnd = duration;
    }

    public Status UpdatePart()
    {
        if (timer >= partEnd) return Status.Success;
        timer += Time.deltaTime;
        return timer >= partEnd ? Status.Success : Status.Running;
    }

    // 4. Pedir ayuda a Ramón y esperarla (percepción push: tarea completada)
    public void StartAskRamon()
    {
        waitingTask = null;
        if (CurrentActivity.interactionMode == ActivityInteractionMode.None) return;

        TaskLogic task = FindOwnTask(CurrentActivity.interactionTask);
        if (task == null)
        {
            Debug.LogWarning(name + ": no tiene un TaskLogic hijo con " + CurrentActivity.interactionTask, this);
            return;
        }

        if (TaskManager.Instance != null && TaskManager.Instance.TryActivate(task, profile.alwaysGeneratesTasks))
        {
            waitingTask = task;
            Log("Pide ayuda a Ramón: " + task.Data.displayName);
        }
    }

    public Status UpdateAskRamon()
    {
        // Running mientras la tarea siga activa. Se acaba al completarla o si caduca.
        return waitingTask != null && waitingTask.isActive ? Status.Running : Status.Success;
    }

    // 6. Generar tarea: la tarea es la del propio punto de destino
    public Status GenerateTask()
    {
        TaskLogic task = destination != null? 
            destination.GetTaskForSlot(navigator.CurrentDestinationSlot) : null;
        if (task != null && TaskManager.Instance != null &&
            TaskManager.Instance.TryActivate(task, profile.alwaysGeneratesTasks))
        {
            Log("Genera tarea " + task.Data.displayName + " en " + destination.DisplayName);
        }
        else
        {
            Log("No genera tarea");
        }
        return Status.Success;
    }

    // 7. Liberar destino y registrar en el historial
    public Status FinishActivity()
    {
        SetAnimation(false);
        navigator.ReleaseCurrentDestination();
        destination = null;

        LastActivity = CurrentActivity;
        lastFinishTime[CurrentActivity] = Time.time;
        return Status.Success;
    }

    // Llamado por StudentBehaviour cuando el US termina (éxito o fallo) o se desactiva.
    // Se puede llamar aunque FinishActivity ya haya limpiado.
    public void EndActivity(bool success)
    {
        if (CurrentActivity == null) return;

        ActivityData finished = CurrentActivity;

        SetAnimation(false);
        navigator.CancelCurrentTrip(); // Para y libera si quedaba algo reservado
        destination = null;
        waitingTask = null;

        if (!success) blockedUntil[finished] = Time.time + FailCooldown;

        CurrentActivity = null;
        RollNoise();

        Log("Termina " + finished.name + (success ? "" : " (fallo)"));
        OnActivityFinished?.Invoke(this, finished);
    }

    // ==================================================================
    // IDLE: acción de reserva si nada tiene utilidad
    // ==================================================================
    public void StartIdle() => idleTimer = UnityEngine.Random.Range(1f, 2.5f);

    public Status UpdateIdle()
    {
        idleTimer -= Time.deltaTime;
        return idleTimer <= 0f ? Status.Success : Status.Running;
    }

    // ==================================================================
    // Visibilidad (entidades)
    // ==================================================================
    // No desactivéis el GameObject al aparecer la entidad: NPCFloorNavigator.OnDisable
    // cancela el viaje y la especificación pide que la simulación siga.
    public void SetVisible(bool visible)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
            r.enabled = visible;
    }

    // ==================================================================
    // Internos
    // ==================================================================

    // Primero la planta actual, luego las demás en orden aleatorio
    private List<BuildingFloor> GetFloorsToSearch()
    {
        List<BuildingFloor> floors = new List<BuildingFloor> { navigator.CurrentFloor };
        List<BuildingFloor> rest = new List<BuildingFloor>();

        foreach (BuildingFloor f in Enum.GetValues(typeof(BuildingFloor)))
            if (f != navigator.CurrentFloor) rest.Add(f);

        while (rest.Count > 0)
        {
            int i = UnityEngine.Random.Range(0, rest.Count);
            floors.Add(rest[i]);
            rest.RemoveAt(i);
        }
        return floors;
    }

    private TaskLogic FindOwnTask(TaskData data)
    {
        if (data == null) return null;
        foreach (TaskLogic t in ownTasks)
            if (t != null && t.Data == data) return t;
        return null;
    }

    private void HandleArrived(NPCDestination reached)
    {
        if (CurrentActivity != null && reached == destination) arrived = true;
    }

    private void HandleMoveFailed(NPCDestination failed)
    {
        if (CurrentActivity != null && destination != null && failed == destination && !arrived)
            moveFailed = true;
    }

    private void SetAnimation(bool on)
    {
        if (animator == null || CurrentActivity == null || string.IsNullOrEmpty(CurrentActivity.animatorBool))
            return;

        foreach (AnimatorControllerParameter p in animator.parameters)
        {
            if (p.name == CurrentActivity.animatorBool && p.type == AnimatorControllerParameterType.Bool)
            {
                animator.SetBool(p.nameHash, on);
                return;
            }
        }
    }

    private void Log(string message)
    {
        if (debugLogs) Debug.Log("[" + name + "] " + message, this);
    }
}
