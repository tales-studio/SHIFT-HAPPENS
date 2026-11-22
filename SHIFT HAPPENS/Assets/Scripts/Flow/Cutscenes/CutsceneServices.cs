using UnityEngine;

public static class CutsceneServices
{
    private static ICutscenePlayer player;

    public static ICutscenePlayer Player => player ?? NullCutscenePlayer.Instance;

    public static void Register(ICutscenePlayer cutscenePlayer)
    {
        player = cutscenePlayer;
    }

    public static void Unregister(object owner)
    {
        if (ReferenceEquals(player, owner)) player = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        player = null;
    }
}
