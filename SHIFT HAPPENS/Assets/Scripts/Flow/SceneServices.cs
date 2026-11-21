using UnityEngine;

public static class SceneServices
{
    private static IScreenFader fader;
    private static ISceneLoader loader;

    public static IScreenFader Fader => fader ?? NullScreenFader.Instance;
    public static ISceneLoader Loader => loader ?? NullSceneLoader.Instance;

    public static void Register(IScreenFader screenFader)
    {
        fader = screenFader;
    }

    public static void Register(ISceneLoader sceneLoader)
    {
        loader = sceneLoader;
    }

    public static void Unregister(object owner)
    {
        if (ReferenceEquals(fader, owner)) fader = null;
        if (ReferenceEquals(loader, owner)) loader = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        fader = null;
        loader = null;
    }
}
