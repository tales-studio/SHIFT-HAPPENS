using UnityEngine;

public static class SceneServices
{
    private static IScreenFader fader;
    private static ISceneLoader loader;
    private static ITransitionCard transitionCard;

    public static IScreenFader Fader => fader ?? NullScreenFader.Instance;
    public static ISceneLoader Loader => loader ?? NullSceneLoader.Instance;
    public static ITransitionCard TransitionCard => transitionCard ?? NullTransitionCard.Instance;

    public static void Register(IScreenFader screenFader)
    {
        fader = screenFader;
    }

    public static void Register(ISceneLoader sceneLoader)
    {
        loader = sceneLoader;
    }

    public static void Register(ITransitionCard card)
    {
        transitionCard = card;
    }

    public static void Unregister(object owner)
    {
        if (ReferenceEquals(fader, owner)) fader = null;
        if (ReferenceEquals(loader, owner)) loader = null;
        if (ReferenceEquals(transitionCard, owner)) transitionCard = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        fader = null;
        loader = null;
        transitionCard = null;
    }
}
