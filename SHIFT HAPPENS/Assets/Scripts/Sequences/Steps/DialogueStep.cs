using UnityEngine;

public class DialogueStep : SequenceStep
{
    [SerializeField] private DialogueData dialogue;

    protected override void OnEnter()
    {
        DialogueServices.Presenter.Play(dialogue, Complete);
    }

    protected override void OnExit()
    {
        DialogueServices.Presenter.Hide();
    }
}
