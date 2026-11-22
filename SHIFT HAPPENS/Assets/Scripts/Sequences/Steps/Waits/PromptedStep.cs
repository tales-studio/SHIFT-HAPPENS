using UnityEngine;

public abstract class PromptedStep : SequenceStep
{
    [SerializeField] private DialogueData prompt;

    protected sealed override void OnEnter()
    {
        if (prompt != null) DialogueServices.Presenter.Play(prompt, null, true);
        BeginWait();
    }

    protected sealed override void OnExit()
    {
        EndWait();
        if (prompt != null) DialogueServices.Presenter.Hide();
    }

    protected abstract void BeginWait();

    protected virtual void EndWait() { }
}
