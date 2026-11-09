using System;
using UnityEngine;

public class AnxietyManager : MonoBehaviour
{
    public static AnxietyManager Instance { get; private set; }

    [SerializeField] private float anxietyAmmount;

    private float maxAnxiety = 100f;
    [SerializeField] private float activeTaskAnxiety = 1f;
    [SerializeField] private float noTasksDecay = 5f;

    public float NormalizedAnxiety
    {
        get
        {
            return anxietyAmmount / maxAnxiety;
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
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnTaskCompleted += HandleTaskCompleted;
            TaskManager.Instance.OnTaskFailed += HandleTaskFailed;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged += HandlePhaseChanged;
        }
    }

    private void OnDestroy()
    {
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnTaskCompleted -= HandleTaskCompleted;
            TaskManager.Instance.OnTaskFailed -= HandleTaskFailed;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (!IsAnxietyActive()) return;

        int activeTasks = TaskManager.Instance.ActiveTaskCount;

        if (activeTasks > 0)
        {
            IncreaseAnxiety(activeTaskAnxiety * activeTasks * Time.deltaTime);
        }
        else
        {
            DecreaseAnxiety(noTasksDecay * Time.deltaTime);
        }
    }

    private bool IsAnxietyActive()
    {
        if (GameManager.Instance == null) return false;

        GamePhase phase = GameManager.Instance.CurrentPhase;
        return phase == GamePhase.Normal || phase == GamePhase.Blackout || phase == GamePhase.Outbreak;
    }

    private void HandlePhaseChanged(GamePhase phase)
    {
        anxietyAmmount = 0;
    }

    private void HandleTaskFailed(TaskLogic logic)
    {
        if (!IsAnxietyActive()) return;
        IncreaseAnxiety(logic.Data.anxietyOnFail);
    }

    private void HandleTaskCompleted(TaskLogic logic)
    {
        if (!IsAnxietyActive()) return;
        DecreaseAnxiety(logic.Data.reliefOnComplete);
    }

    private void IncreaseAnxiety(float ammount)
    {
        anxietyAmmount = Mathf.Clamp(anxietyAmmount + ammount, 0f, maxAnxiety);
    }

    private void DecreaseAnxiety(float ammount)
    {
        anxietyAmmount = Mathf.Clamp(anxietyAmmount - ammount, 0f, maxAnxiety);
    }


}
