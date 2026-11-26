using UnityEngine;

public static class FootstepSurfaces
{
    private static IFootstepSurfaceProvider provider;

    public static IFootstepSurfaceProvider Provider => provider ?? NullFootstepSurfaceProvider.Instance;

    public static void Register(IFootstepSurfaceProvider surfaceProvider)
    {
        provider = surfaceProvider;
    }

    public static void Unregister(IFootstepSurfaceProvider surfaceProvider)
    {
        if (ReferenceEquals(provider, surfaceProvider)) provider = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        provider = null;
    }
}
