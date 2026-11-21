using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.StateMachines;
using BehaviourAPI.UnityToolkit.GUIDesigner.Runtime;
using UnityEngine;

// Monta y ejecuta la FSM de la entidad con la BehaviourAPI. Es la misma para
// todos los arquetipos: lo que cambia está en la subclase de EntityBrain.
[RequireComponent(typeof(EntityBrain))]
public class EntityBehaviour : MonoBehaviour
{
    [Tooltip("Opcional: para ver la FSM en el editor en modo Debug.")]
    [SerializeField] private BSRuntimeDebugger runtimeDebugger;

    private EntityBrain brain;
    private FSM fsm;
    private PushPerception interruptPerception;
    private bool finished;

    private void Awake()
    {
        brain = GetComponent<EntityBrain>();
        brain.InterruptRequested += HandleInterrupt;
    }

    // En Start: el controller ya ha llamado a brain.Begin() tras instanciarla
    private void Start()
    {
        fsm = BuildFSM();

        if (runtimeDebugger != null)
            runtimeDebugger.RegisterGraph(fsm, "FSM " + name);

        fsm.Start();
    }

    private void Update()
    {
        if (fsm == null || finished) return;

        fsm.Update();

        // La transición de salida (desde Desaparecer) termina la FSM
        if (fsm.Status == Status.Success || fsm.Status == Status.Failure)
        {
            finished = true;
            fsm.Stop();
            brain.NotifyFinished(); // El controller vuelve a mostrar al estudiante y la destruye
        }
    }

    private void OnDestroy()
    {
        if (brain != null) brain.InterruptRequested -= HandleInterrupt;
        if (fsm != null && !finished && fsm.Status != Status.None) fsm.Stop();
    }

    private void HandleInterrupt()
    {
        interruptPerception?.Fire();
    }

    private FSM BuildFSM()
    {
        FSM machine = new FSM();

        // Estados: cada uno ejecuta funciones de EntityBrain
        State appear = machine.CreateState("Aparecer",
            new FunctionalAction(brain.StartAppear, brain.UpdateAppear, null));

        State chase = machine.CreateState("Perseguir",
            new FunctionalAction(brain.StartChase, brain.UpdateChase, brain.StopChase));

        State attack = machine.CreateState("Atacar",
            new FunctionalAction(brain.StartAttack, brain.UpdateAttack, null));

        State disappear = machine.CreateState("Desaparecer",
            new FunctionalAction(brain.StartDisappear, brain.UpdateDisappear, null));

        // Transiciones push: StatusFlags.None = nunca se comprueban solas,
        // solo se lanzan con interruptPerception.Fire()
        StateTransition interruptFromAppear = machine.CreateTransition("Interrumpida al aparecer",
            appear, disappear, statusFlags: StatusFlags.None);

        StateTransition interruptFromChase = machine.CreateTransition("Interrumpida",
            chase, disappear, statusFlags: StatusFlags.None);

        if (brain.UsesWatchState)
        {
            // Aparecer → Acechar → (de pie) Perseguir / (agachado el tiempo suficiente) Desaparecer
            State watch = machine.CreateState("Acechar",
                new FunctionalAction(brain.StartWatch, brain.UpdateWatch, brain.StopWatch));

            machine.CreateTransition("Aparicion terminada", appear, watch,
                statusFlags: StatusFlags.Finished);

            machine.CreateTransition("Ramon de pie", watch, chase,
                new ConditionPerception(brain.ShouldStartChase));

            machine.CreateTransition("Ramon agachado el tiempo suficiente", watch, disappear,
                statusFlags: StatusFlags.Finished);

            StateTransition interruptFromWatch = machine.CreateTransition("Interrumpida al acechar",
                watch, disappear, statusFlags: StatusFlags.None);

            interruptPerception = new PushPerception(interruptFromAppear, interruptFromWatch, interruptFromChase);
        }
        else
        {
            machine.CreateTransition("Aparicion terminada", appear, chase,
                statusFlags: StatusFlags.Finished);

            interruptPerception = new PushPerception(interruptFromAppear, interruptFromChase);
        }

        // Transición pull de escape (escondido o, en el bully, demasiado lejos)
        machine.CreateTransition("Ramon escapa", chase, disappear,
            new ConditionPerception(brain.ShouldEscape));

        // Transición pull: se comprueba cada frame
        machine.CreateTransition("Ramon en rango", chase, attack,
            new ConditionPerception(brain.PlayerInAttackRange));

        // Transición al terminar el estado (sin percepción)
        machine.CreateTransition("Ataque terminado", attack, disappear,
            statusFlags: StatusFlags.Finished);

        // Salida: al terminar Desaparecer, la FSM acaba con éxito
        machine.CreateExitTransition("Fin del encuentro", disappear, Status.Success,
            statusFlags: StatusFlags.Finished);

        machine.SetEntryState(appear);
        return machine;
    }
}
