using System;

public sealed class NullDialoguePresenter : IDialoguePresenter
{
    public static readonly NullDialoguePresenter Instance = new NullDialoguePresenter();

    private NullDialoguePresenter() { }

    public bool IsShowing => false;

    public void Play(DialogueData data, Action finished = null, bool holdLast = false)
    {
        finished?.Invoke();
    }

    public void Hide() { }
}
