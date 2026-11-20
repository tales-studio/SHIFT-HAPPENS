using System.Collections.Generic;
using UnityEngine;

// Versión mínima para poder probar las entidades. Más adelante se completará.
//
// Responsabilidades:
//  - Decidir si se pueden crear entidades (fase Outbreak). Sin límite de cantidad;
//    cada estudiante solo puede tener una entidad a la vez.
//  - Crearla cuando un EntityTrigger lo pide, ocultando a su estudiante si toca.
//  - Cuando la entidad termina: destruirla, mostrar al estudiante e iniciar su cooldown.
//  - Interrumpir las entidades si Ramón cambia de planta.
public class HallucinationController : MonoBehaviour
{
    public static HallucinationController Instance { get; private set; }

    [Tooltip("Desmárcalo para probar las entidades en cualquier fase.")]
    [SerializeField] private bool onlyDuringOutbreak = true;

    [SerializeField] private bool debugLogs = false;

    // Entidad activa → trigger (estudiante) que la originó
    private readonly Dictionary<EntityBrain, EntityTrigger> activeEntities =
        new Dictionary<EntityBrain, EntityTrigger>();

    public int ActiveEntityCount => activeEntities.Count;

    public bool CanSpawn
    {
        get
        {
            if (Player.Instance == null) return false;
            if (GameManager.Instance != null && !GameManager.Instance.isPlaying) return false;
            if (onlyDuringOutbreak &&
                (GameManager.Instance == null || GameManager.Instance.CurrentPhase != GamePhase.Outbreak))
                return false;
            return true;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (FadeManager.Instance != null)
            FadeManager.Instance.OnTransitionStarted += HandlePlayerChangesFloor;
    }

    private void OnDestroy()
    {
        if (FadeManager.Instance != null)
            FadeManager.Instance.OnTransitionStarted -= HandlePlayerChangesFloor;
        if (Instance == this) Instance = null;
    }

    // Percepción push "Activación": la pide el EntityTrigger del estudiante
    public bool RequestSpawn(EntityTrigger trigger)
    {
        if (!CanSpawn || trigger == null || trigger.Config == null || trigger.IsEntityActive)
            return false;

        GameObject prefab = trigger.Config.entityPrefab;
        if (prefab == null)
        {
            Debug.LogWarning("EntityConfig " + trigger.Config.name + " no tiene prefab.", trigger);
            return false;
        }

        if (!trigger.TryGetSpawnPosition(Player.Instance, out Vector3 position))
            return false;

        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        EntityBrain entity = instance.GetComponent<EntityBrain>();
        if (entity == null)
        {
            Debug.LogError("El prefab " + prefab.name + " no tiene un EntityBrain.", prefab);
            Destroy(instance);
            return false;
        }

        activeEntities.Add(entity, trigger);
        trigger.IsEntityActive = true;

        // El estudiante se oculta pero sigue simulando (no se desactiva)
        if (trigger.Config.hidesStudent)
            trigger.Student.SetVisible(false);

        entity.Finished += HandleEntityFinished;
        entity.Begin(trigger.Config, trigger.Student, Player.Instance);

        Log("Aparece la entidad de " + trigger.name);
        return true;
    }

    // La entidad ha terminado (ha atacado o ha sido repelida)
    private void HandleEntityFinished(EntityBrain entity, bool caughtPlayer)
    {
        entity.Finished -= HandleEntityFinished;

        if (activeEntities.TryGetValue(entity, out EntityTrigger trigger))
        {
            activeEntities.Remove(entity);

            // Comprobación de null: al cerrar la escena pueden estar destruidos
            if (trigger != null && trigger.Student != null)
            {
                if (trigger.Config.hidesStudent)
                    trigger.Student.SetVisible(true);

                trigger.StartCooldown();
                trigger.IsEntityActive = false;
                Log("Termina la entidad de " + trigger.name + (caughtPlayer ? " (atrapó a Ramón)" : " (sin ataque)"));
            }
        }

        if (entity != null) Destroy(entity.gameObject);
    }

    // Si Ramón cambia de planta, todas las entidades desaparecen
    private void HandlePlayerChangesFloor()
    {
        List<EntityBrain> entities = new List<EntityBrain>(activeEntities.Keys);
        foreach (EntityBrain entity in entities)
        {
            if (entity != null) entity.Interrupt();
        }
    }

    private void Log(string message)
    {
        if (debugLogs) Debug.Log("[HallucinationController] " + message, this);
    }
}
