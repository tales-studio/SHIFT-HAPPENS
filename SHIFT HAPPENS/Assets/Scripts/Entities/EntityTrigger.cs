using UnityEngine;

[RequireComponent(typeof(StudentBrain))]
public class EntityTrigger : MonoBehaviour
{
    [SerializeField] private EntityConfig config;

    [Tooltip("Cada cuánto se comprueba la condición de aparición.")]
    [SerializeField, Min(0.02f)] private float checkInterval = 0.2f;

    private StudentBrain student;
    private float lastDespawnTime = float.NegativeInfinity;
    private float nextCheckTime;

    public EntityConfig Config => config;
    public StudentBrain Student => student;

    // Lo marca el HallucinationController mientras su entidad está en escena
    public bool IsEntityActive { get; set; }

    public bool IsOnCooldown =>
        config != null && Time.time < lastDespawnTime + config.reactivationCooldown;

    protected virtual void Awake()
    {
        student = GetComponent<StudentBrain>();
    }

    private void Update()
    {
        HallucinationController controller = HallucinationController.Instance;
        if (controller == null || !controller.CanSpawn) return;

        Tick(Time.deltaTime);

        if (Time.time < nextCheckTime) return;
        nextCheckTime = Time.time + checkInterval;

        if (CanActivate(Player.Instance))
            controller.RequestSpawn(this);
    }

    // Se llama cada frame mientras se pueden crear entidades (temporizadores propios)
    protected virtual void Tick(float deltaTime) { }

    // Condición de aparición por defecto: Ramón cerca del estudiante
    public virtual bool CanActivate(Player player)
    {
        if (config == null || player == null) return false;
        if (IsEntityActive || IsOnCooldown) return false;
        if (student.BlocksEntity) return false; // emo llorando

        // Agachado se acerca más sin activarla (o nunca, si el rango es 0)
        float range = player.IsSneaking ? config.sneakActivationRange : config.activationRange;
        if (range <= 0f) return false;

        Vector2 offset = player.Position - transform.position;
        return offset.sqrMagnitude <= range * range;
    }

    // Dónde aparece la entidad. Por defecto, en el estudiante.
    public virtual bool TryGetSpawnPosition(Player player, out Vector3 position)
    {
        position = transform.position;
        return true;
    }

    public virtual void StartCooldown()
    {
        lastDespawnTime = Time.time;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (config == null) return;
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, config.activationRange);
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, config.sneakActivationRange);
    }
}
