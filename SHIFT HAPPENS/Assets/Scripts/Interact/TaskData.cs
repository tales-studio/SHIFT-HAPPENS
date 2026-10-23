using UnityEngine;

[CreateAssetMenu(fileName = "NewTask", menuName = "Shift Happens/Task Data")]
public class TaskData : ScriptableObject
{
        public string displayName;
        public float expirationSeconds;
        public float anxietyOnFail;
}
