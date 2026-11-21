using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.UnityToolkit.GUIDesigner.Runtime;
using BehaviourAPI.UtilitySystems;
using UnityEngine;

// Monta y ejecuta el comportamiento del estudiante con la BehaviourAPI:
//
//   Utility System: una UtilityAction por actividad + Idle.
//   Behaviour Tree: UNO solo, compartido. Cada UtilityAction le dice al
//   cerebro qué actividad toca (SetActivity) y arranca ese mismo árbol.
//
// Toda la lógica está en StudentBrain; aquí solo se conecta.
[RequireComponent(typeof(StudentBrain))]
public class StudentBehaviour : MonoBehaviour
{
    private const float NeedCurveExponent = 2f; // la necesidad crece tarde y de golpe

    [SerializeField] private float inertia = 1.3f;
    [SerializeField, Range(0f, 0.3f)] private float idleUtility = 0.05f;

    [Tooltip("Opcional: para ver los grafos en el editor visual en modo Debug.")]
    [SerializeField] private BSRuntimeDebugger runtimeDebugger;

    private StudentBrain brain;
    private UtilitySystem utilitySystem;
    private BehaviourTree activityTree;

    private void Start()
    {
        // En Start (no en Awake) para que StudentBrain ya tenga su lista de actividades
        brain = GetComponent<StudentBrain>();
        activityTree = BuildActivityTree();
        utilitySystem = BuildUtilitySystem();

        if (runtimeDebugger != null)
        {
            runtimeDebugger.RegisterGraph(utilitySystem, "US " + name);
            runtimeDebugger.RegisterGraph(activityTree, "BT actividad");
        }
    }

    private void Update()
    {
        if (utilitySystem == null) return;
        if (GameManager.Instance != null && !GameManager.Instance.isPlaying) return;

        if (utilitySystem.Status == Status.None)
            utilitySystem.Start();

        utilitySystem.Update();

        // Las UtilityAction tienen finishOnComplete = true: al acabar una
        // actividad termina el US. Limpiamos y reiniciamos para decidir de nuevo.
        if (utilitySystem.Status == Status.Success || utilitySystem.Status == Status.Failure)
        {
            brain.EndActivity(utilitySystem.Status == Status.Success);
            utilitySystem.Restart();
        }
    }

    private void OnDisable()
    {
        if (utilitySystem != null && utilitySystem.Status != Status.None)
            utilitySystem.Stop();

        if (brain != null)
            brain.EndActivity(false);
    }

    // ==================================================================
    // UTILITY SYSTEM
    // ==================================================================
    //  Con necesidad (baño, cocina):
    //    Preferencia (Constant) ──────────────────────────┐
    //    Tiempo desde la última (Variable) ─ Exponencial ─┴─ Ponderada ─ Modificadores ─ Acción
    //  Sin necesidad (resto):
    //    Preferencia (Constant) ─ Modificadores ─ Acción
    private UtilitySystem BuildUtilitySystem()
    {
        UtilitySystem us = new UtilitySystem(inertia);

        foreach (ActivityEntry entry in brain.Activities)
        {
            ActivityData data = entry.activity;
            string id = data.name;

            Factor factor = us.CreateConstant(id + " preferencia", entry.preference);

            if (data.timeToFullNeed > 0f)
            {
                VariableFactor time = us.CreateVariable(id + " tiempo",
                    () => brain.GetTimeSinceLast(data), 0f, data.timeToFullNeed);

                ExponentialCurveFactor need = us.CreateCurve<ExponentialCurveFactor>(id + " necesidad", time);
                need.Exponent = NeedCurveExponent;

                WeightedFusionFactor fusion = us.CreateFusion<WeightedFusionFactor>(id + " mezcla", factor, need);
                fusion.Weights = new float[] { 1f - data.needWeight, data.needWeight };
                factor = fusion;
            }

            CustomCurveFactor final = us.CreateCurve<CustomCurveFactor>(id + " modificadores", factor);
            final.Function = x => brain.ApplyModifiers(data, x);

            // La acción: fijar la actividad y ejecutar el BT compartido
            FunctionalAction runActivity = new FunctionalAction(
                () => { brain.SetActivity(data); activityTree.Start(); },
                () => { activityTree.Update(); return activityTree.Status; },
                () => { if (activityTree.Status != Status.None) activityTree.Stop(); });

            us.CreateAction(id, final, runActivity, true);
        }

        // Acción por defecto si nada es posible (baños ocupados, etc.)
        ConstantFactor idleFactor = us.CreateConstant("Idle utilidad", idleUtility);
        us.CreateAction("Idle", idleFactor, new FunctionalAction(brain.StartIdle, brain.UpdateIdle, null), true);

        return us;
    }

    // ==================================================================
    // BEHAVIOUR TREE GENÉRICO (uno para todas las actividades)
    // ==================================================================
    private BehaviourTree BuildActivityTree()
    {
        BehaviourTree bt = new BehaviourTree();

        LeafNode request = bt.CreateLeafNode("Solicitar destino",
            new FunctionalAction(null, brain.RequestDestination, null));

        LeafNode move = bt.CreateLeafNode("Desplazarse",
            new FunctionalAction(null, brain.UpdateMovement, null));

        LeafNode firstPart = bt.CreateLeafNode("Realizar actividad (1a parte)",
            new FunctionalAction(brain.StartFirstPart, brain.UpdatePart, null));

        LeafNode askRamon = bt.CreateLeafNode("Pedir ayuda y esperar",
            new FunctionalAction(brain.StartAskRamon, brain.UpdateAskRamon, null));

        LeafNode secondPart = bt.CreateLeafNode("Realizar actividad (2a parte)",
            new FunctionalAction(brain.StartSecondPart, brain.UpdatePart, null));

        LeafNode generateTask = bt.CreateLeafNode("Generar tarea",
            new FunctionalAction(null, brain.GenerateTask, null));

        LeafNode finish = bt.CreateLeafNode("Liberar destino y registrar",
            new FunctionalAction(null, brain.FinishActivity, null));

        SequencerNode root = bt.CreateComposite<SequencerNode>("Actividad", false,
            request, move, firstPart, askRamon, secondPart, generateTask, finish);

        bt.SetRootNode(root);
        return bt;
    }
}
