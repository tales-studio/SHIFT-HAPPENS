using System;

public interface IDialoguePresenter
{
    bool IsShowing { get; }

    void Play(DialogueData data, Action finished = null, bool holdLast = false);
    void Hide();
}
