using UnityEngine;

public static class DialogueServices
{
    private static IDialoguePresenter presenter;

    public static IDialoguePresenter Presenter => presenter ?? NullDialoguePresenter.Instance;

    public static bool IsAvailable => presenter != null;

    public static void Register(IDialoguePresenter dialoguePresenter)
    {
        presenter = dialoguePresenter;
    }

    public static void Unregister(object owner)
    {
        if (ReferenceEquals(presenter, owner)) presenter = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        presenter = null;
    }
}
