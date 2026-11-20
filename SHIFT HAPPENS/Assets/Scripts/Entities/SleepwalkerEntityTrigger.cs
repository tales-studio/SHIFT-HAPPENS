using UnityEngine;
using UnityEngine.AI;

public class SleepwalkerEntityTrigger : EntityTrigger
{
    [Header("Posición de aparición")]
    [Tooltip("Distancia extra fuera del borde de la cámara.")]
    [SerializeField, Min(0f)] private float offscreenMargin = 1f;

    [Tooltip("Rango de distancias, a partir del borde, en el que se buscan puntos.")]
    [SerializeField, Min(0f)] private float spawnSpread = 6f;

    [SerializeField, Min(1)] private int spawnAttempts = 25;

    private float timeUntilSpawn;

    protected override void Awake()
    {
        base.Awake();
        ScheduleNextSpawn();
    }

    // Solo cuenta mientras se pueden crear entidades (Outbreak)
    protected override void Tick(float deltaTime)
    {
        if (!IsEntityActive) timeUntilSpawn -= deltaTime;
    }

    public override bool CanActivate(Player player)
    {
        if (Config == null || player == null) return false;
        if (IsEntityActive || IsOnCooldown) return false;
        return timeUntilSpawn <= 0f;
    }

    public override void StartCooldown()
    {
        base.StartCooldown();
        ScheduleNextSpawn();
    }

    private void ScheduleNextSpawn()
    {
        if (Config == null) return;
        timeUntilSpawn = Random.Range(Config.spawnIntervalMin,
            Mathf.Max(Config.spawnIntervalMin, Config.spawnIntervalMax));
    }

    // Busca un punto del NavMesh fuera de la cámara y conectado con Ramón.
    public override bool TryGetSpawnPosition(Player player, out Vector3 position)
    {
        position = transform.position;

        Camera cam = Camera.main;
        if (cam == null || player == null) return false;

        NavMeshQueryFilter filter = new NavMeshQueryFilter { areaMask = NavMesh.AllAreas };
        NavMeshAgent prefabAgent = Config.entityPrefab != null
            ? Config.entityPrefab.GetComponent<NavMeshAgent>()
            : null;
        if (prefabAgent != null)
        {
            filter.agentTypeID = prefabAgent.agentTypeID;
            filter.areaMask = prefabAgent.areaMask;
        }

        if (!NavMesh.SamplePosition(player.Position, out NavMeshHit playerHit, 1.5f, filter))
            return false;

        float halfHeight = cam.orthographic ? cam.orthographicSize : 5f;
        float halfWidth = halfHeight * cam.aspect;
        float minDistance = Mathf.Sqrt(halfWidth * halfWidth + halfHeight * halfHeight) + offscreenMargin;
        float maxDistance = minDistance + spawnSpread;

        NavMeshPath path = new NavMeshPath();
        bool foundFallback = false;
        float fallbackDistance = 0f;
        Vector3 fallback = Vector3.zero;

        for (int i = 0; i < spawnAttempts; i++)
        {
            Vector2 direction = Random.insideUnitCircle.normalized;
            float distance = Random.Range(minDistance * 0.5f, maxDistance);
            Vector3 candidate = playerHit.position + (Vector3)(direction * distance);

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, filter))
                continue;

            if (!NavMesh.CalculatePath(playerHit.position, hit.position, filter, path) ||
                path.status != NavMeshPathStatus.PathComplete)
                continue; // Otra planta o zona aislada

            Vector3 viewport = cam.WorldToViewportPoint(hit.position);
            bool offscreen = viewport.x < 0f || viewport.x > 1f || viewport.y < 0f || viewport.y > 1f;
            if (offscreen)
            {
                position = hit.position;
                return true;
            }

            float fromPlayer = Vector2.Distance(hit.position, playerHit.position);
            if (fromPlayer > fallbackDistance)
            {
                fallbackDistance = fromPlayer;
                fallback = hit.position;
                foundFallback = true;
            }
        }

        position = fallback;
        return foundFallback;
    }

    protected override void OnDrawGizmosSelected() { }
}
