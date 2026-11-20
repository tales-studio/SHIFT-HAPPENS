using UnityEngine;

public class TaskSpriteSwap : MonoBehaviour
{
    [SerializeField] private TaskLogic task;
    [SerializeField] private TwoStateVisual visual;

    private void Reset()
    {
        task = GetComponent<TaskLogic>();
        visual = GetComponentInChildren<TwoStateVisual>(true);
    }

    private void OnEnable()
    {
        if (task == null || visual == null)
        {
            Debug.LogWarning($"{name}: TaskSpriteSwap is missing its Task or Visual reference.", this);
            return;
        }

        task.ActiveChanged += visual.SetAlternateState;
        visual.SetAlternateState(task.isActive);
    }

    private void OnDisable()
    {
        if (task != null && visual != null)
            task.ActiveChanged -= visual.SetAlternateState;
    }
}
