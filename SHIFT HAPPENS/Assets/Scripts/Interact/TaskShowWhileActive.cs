using UnityEngine;

public class TaskShowWhileActive : MonoBehaviour
{
    [SerializeField] private TaskLogic task;
    [SerializeField] private GameObject target;

    private void Reset()
    {
        task = GetComponent<TaskLogic>();
    }

    private void OnEnable()
    {
        if (task == null || target == null)
        {
            Debug.LogWarning($"{name}: TaskShowWhileActive is missing its Task or Target reference.", this);
            return;
        }

        task.ActiveChanged += target.SetActive;
        target.SetActive(task.isActive);
    }

    private void OnDisable()
    {
        if (task != null && target != null)
            task.ActiveChanged -= target.SetActive;
    }
}
