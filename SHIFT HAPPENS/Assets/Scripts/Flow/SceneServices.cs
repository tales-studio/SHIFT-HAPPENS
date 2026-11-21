using UnityEngine;

public static class SceneServices
{
    private static IScreenFader fader;

    public static IScreenFader Fader => fader ?? NullScreenFader.Instance;

    public static void Register(IScreenFader screenFader)
    {
        fader = screenFader;
    }

    public static void Unregister(object owner)
    {
        if (ReferenceEquals(fader, owner)) fader = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        fader = null;
    }
}
