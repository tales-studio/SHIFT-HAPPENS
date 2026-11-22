using UnityEngine;

public enum EntityArchetype
{
    Nerd,
    Bully,
    Emo,
    Sleepwalker
}

// Configuración de una entidad por arquetipo (especificación 3.5).
// Un asset por arquetipo, compartido por todos los estudiantes de ese tipo.
[CreateAssetMenu(fileName = "NewEntityConfig", menuName = "Shift Happens/Entity Config")]
public class EntityConfig : ScriptableObject
{
    public EntityArchetype archetype;

    [Tooltip("Prefab por defecto (masculino en los arquetipos con variantes). Debe incluir EntityBrain y NavMeshAgent.")]
    public GameObject entityPrefab;

    [Tooltip("Variante femenina opcional. Si se deja vacía se usa el prefab por defecto.")]
    public GameObject femaleEntityPrefab;

    public GameObject GetEntityPrefab(StudentGender gender)
    {
        return gender == StudentGender.Female && femaleEntityPrefab != null
            ? femaleEntityPrefab
            : entityPrefab;
    }

    [Tooltip("Ocultar al estudiante mientras su entidad está en escena.")]
    public bool hidesStudent = true;

    [Header("Activación por proximidad (todas menos el sonámbulo)")]
    [Tooltip("Distancia a la que Ramón activa la entidad yendo de pie.")]
    [Min(0f)] public float activationRange = 3f;

    [Tooltip("Distancia a la que la activa yendo agachado. 0 = agachado nunca la activa.")]
    [Min(0f)] public float sneakActivationRange = 1.5f;

    [Tooltip("Segundos que deben pasar para que la entidad de este estudiante vuelva a aparecer.")]
    [Min(0f)] public float reactivationCooldown = 30f;

    [Header("Persecución y ataque")]
    [Tooltip("Velocidad de persecución. Empollón = la de Ramón (5). Sonámbulo = muy alta.")]
    [Min(0f)] public float chaseSpeed = 5f;

    [Tooltip("Distancia a la que alcanza a Ramón y ataca.")]
    [Min(0f)] public float attackRange = 0.6f;

    [Tooltip("Cada cuánto recalcula el camino hacia Ramón.")]
    [Min(0.05f)] public float repathInterval = 0.2f;

    [Header("Duraciones (segundos)")]
    [Tooltip("En el sonámbulo es lo que dura el grito: el tiempo que tiene Ramón para agacharse.")]
    [Min(0f)] public float appearDuration = 0.6f;
    [Min(0f)] public float attackDuration = 1.5f;
    [Min(0f)] public float disappearDuration = 0.5f;

    [Header("Sonámbulo")]
    [Tooltip("Tiempo aleatorio entre apariciones (se cuenta solo mientras se pueden crear entidades).")]
    [Min(0f)] public float spawnIntervalMin = 40f;
    [Min(0f)] public float spawnIntervalMax = 90f;

    [Tooltip("Segundos que Ramón debe seguir agachado para que el sonámbulo se vaya.")]
    [Min(0f)] public float sneakTimeToVanish = 4f;

    [Header("Bully")]
    [Tooltip("Si Ramón se aleja más de esta distancia mientras le persigue, la entidad desaparece.")]
    [Min(0f)] public float escapeRange = 8f;

    [Header("Emo")]
    [Tooltip("Lo que dura el grito (sustituye a attackDuration).")]
    [Min(0f)] public float screamDuration = 1.5f;

    [Tooltip("Fuerza del temblor de cámara (Cinemachine Impulse).")]
    [Min(0f)] public float shakeForce = 1f;
}
