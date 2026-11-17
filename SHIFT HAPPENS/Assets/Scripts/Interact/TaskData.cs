using UnityEngine;

[CreateAssetMenu(fileName = "NewTask", menuName = "Shift Happens/Task Data")]
public class TaskData : ScriptableObject
{
        public string displayName;
        public bool expires;
        public float expirationSeconds;
        public float anxietyOnFail;
        public float reliefOnComplete;

        [Range(0f, 1f)] public float spawnChance = 0.2f;
}
